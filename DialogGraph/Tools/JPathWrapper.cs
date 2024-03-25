using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using Newtonsoft.Json.Linq;

namespace DialogGraph;

// JToken class constantly creates a new instance of the JPath class which doesn't make much sense. This class is created to cache an instance of the JPath class so that it can be reused again without additional allocations
// https://github.com/JamesNK/Newtonsoft.Json/blob/0a2e291c0d9c0c7675d445703e51750363a549ef/Src/Newtonsoft.Json/Linq/JToken.cs#L2401
public class JPathWrapper
{
    private const string newtonsoftJsonAssemblyName = "Newtonsoft.Json";
    private const string newtonsoftJsonJPathClassName = "JsonPath.JPath";
    private const string newtonsoftJsonJPathEvaluateMethodName = "Evaluate";
    private static Newtonsoft.Json.Formatting makeSureThatNewtonsoftJsonAssemblyHasLoaded;
    
    public static bool isStaticInitialized = false;
    private static Func<string, object> jPathConstructor;
    private static Func<object, JToken, JToken, JsonSelectSettings?,IEnumerable<JToken>> jPathEvaluate;

    private object jPathObj;
    
    public JPathWrapper(string jsonPattern)
    {
        if (isStaticInitialized == false) InitializeStaticDelegates();
        jPathObj = jPathConstructor(jsonPattern);
    }
    
    public IEnumerable<JToken> SelectTokens(JToken jToken, JsonSelectSettings? settings = null)
    {
        var result = jPathEvaluate(jPathObj, jToken, jToken, settings);
        return result;
    }
    
    public JToken SelectFirstToken(JToken jToken, JsonSelectSettings? settings = null)
    {
        var result = jPathEvaluate(jPathObj, jToken, jToken, settings);
        using IEnumerator<JToken> enumerator = result.GetEnumerator();
        if (enumerator.MoveNext())
        {
            return enumerator.Current;
        }
        return null;
    }

    private static void InitializeStaticDelegates()
    {
      makeSureThatNewtonsoftJsonAssemblyHasLoaded = Newtonsoft.Json.Formatting.Indented;
      
      var assemblies = AppDomain.CurrentDomain.GetAssemblies();
      Assembly newtonsoftJsonAssembly = assemblies.FirstOrDefault(assembly => assembly.FullName.Contains(newtonsoftJsonAssemblyName));
      if (newtonsoftJsonAssembly == null) throw new NullReferenceException("The Newtonsoft.Json library was not found among the loaded assemblies. The version or name may have changed");
      
      var jPathType = newtonsoftJsonAssembly.GetTypes().FirstOrDefault(type => type.FullName.Contains(newtonsoftJsonJPathClassName));
      if(jPathType == null) throw new NullReferenceException("JPath class was not found in Newtonsoft.Json library. The name or namespace may have changed");
      MethodInfo evaluateMethod = jPathType.GetMethod(newtonsoftJsonJPathEvaluateMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
      if(evaluateMethod == null) throw new NullReferenceException("Evaluate method was not found in JPath class. The name may have changed");
      VerifyEvaluateMethodSignature(evaluateMethod);
      
      ConstructorInfo constructor = jPathType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, new []{ typeof(String)});
      if(constructor == null) throw new NullReferenceException("The constructor was not found in the JPath class. The signature may have changed");
      VerifyConstructorSignature(constructor);
      
      jPathConstructor = CreateConstructorDelegate(jPathType, constructor);
      jPathEvaluate = CreateEvaluateMethodDelegate(jPathType, evaluateMethod);
      isStaticInitialized = true;
    }

    private static void VerifyEvaluateMethodSignature(MethodInfo evaluateMethod)
    {
        var typesOfArgumentsAndReturnValueForEvaluateMethod = typeof(Func<object, JToken, JToken, JsonSelectSettings?, IEnumerable<JToken>>).GenericTypeArguments;
        var methodReturnType = evaluateMethod.ReturnType;
        var expectedReturnType = typesOfArgumentsAndReturnValueForEvaluateMethod[^1];
        if (methodReturnType != expectedReturnType) throw new ValidationException($"The returned type of the {newtonsoftJsonJPathEvaluateMethodName} method is different from the expected one. Returned type: {methodReturnType.Name}. Expected type: {expectedReturnType.Name}");
        
        var methodParameters = evaluateMethod.GetParameters();
        Span<Type> expectedParameters = new Span<Type>(typesOfArgumentsAndReturnValueForEvaluateMethod, 1, typesOfArgumentsAndReturnValueForEvaluateMethod.Length - 2);
        if(methodParameters.Length != expectedParameters.Length) throw new ValidationException($"The number of parameters of the {newtonsoftJsonJPathEvaluateMethodName} method differs from the expected one. Length: {methodParameters.Length}. Expected length: {expectedParameters.Length}");
        for (int i = 0; i < expectedParameters.Length; i++)
        {
            var methodParamType = methodParameters[i].ParameterType;
            var expectedParamType = expectedParameters[i];
            if (methodParamType != expectedParamType) throw new ValidationException($"The parameter of the {newtonsoftJsonJPathEvaluateMethodName} method at position {i + 1} differs from the expected one. Parameter: {methodParamType.Name}. Expected length: {expectedParamType.Name}");
        }
    }

    private static void VerifyConstructorSignature(ConstructorInfo constructor)
    {
        var constructorParameters = constructor.GetParameters();
        var expectedParaneterType = typeof(String);
        if (constructorParameters.Length != 1) throw new ValidationException("The number of parameters in the constructor differs from the expected one. Parameters length: {constructorParameters.Length}. Expected: 1");
        var constructorParameterType = constructorParameters[0].ParameterType;
        if (constructorParameterType != expectedParaneterType) throw new ValidationException($"The type of the constructor parameter differs from the expected one. Parameter: {constructorParameterType.Name}. Expected: {expectedParaneterType.Name}");
    }
    
    // Works faster than Activator.CreateInstance() and much faster than jPathType.GetConstructor(...).Invoke(...);
    private static Func<string, object> CreateConstructorDelegate(Type jPathType, ConstructorInfo constructor)
    {
        var dynamic = new DynamicMethod("JpathConstructor", jPathType, new Type[]{ typeof(String) }, jPathType);
        ILGenerator il = dynamic.GetILGenerator();

        //il.DeclareLocal(jPathType);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Newobj, constructor);
        il.Emit(OpCodes.Ret);

        var ctor = (Func<string,object>)dynamic.CreateDelegate(typeof(Func<string,object>));
        return ctor;
    }
    
    private static Func<object, JToken,JToken,JsonSelectSettings?,IEnumerable<JToken>> CreateEvaluateMethodDelegate(Type jPathType, MethodInfo evaluateMethod)
    {
        var dynamic = new DynamicMethod("DynamicEvaluateMethod", typeof (IEnumerable<JToken>), new []{typeof(object), typeof(JToken), typeof(JToken), typeof(JsonSelectSettings)}, jPathType);
        ILGenerator il = dynamic.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldarg_3);
        il.Emit(OpCodes.Callvirt, evaluateMethod);
        il.Emit(OpCodes.Ret);
        var func = (Func<object, JToken,JToken,JsonSelectSettings?,IEnumerable<JToken>>) dynamic.CreateDelegate(typeof (Func<object, JToken,JToken,JsonSelectSettings?,IEnumerable<JToken>>));
        return func;
    }
}