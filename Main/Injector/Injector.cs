using System.Diagnostics;
using InjectDotnet;

namespace Main;

public static class Injector
{
    private const string processNameForInjection = "EoCApp";
    private const string runtimeConfigName = "InjectedLogic.runtimeconfig.json";
    private const string nameOfDllToInject = "InjectedLogic.dll";
    private const string assemblyQualifiedTypeName = "InjectedLogic.EntryPointOfInjectedLogic, InjectedLogic, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
    private const string nameOfEntryPointMethod = "Bootstrap";
    private const bool waitForReturnFromInjectedLogic = false;

    private const string hostFxrModuleName = "hostfxr";
    private const string injectedLogicName = "InjectedLogic";
    
    public static void TryInjectLogic()
    {
        var foundProcesses = Process.GetProcessesByName(processNameForInjection);
        switch (foundProcesses.Length)
        {
            case 0:
                throw new ApplicationException($"Game process '{processNameForInjection}' was not found");
            case > 1:
                throw new ApplicationException("More than one game process was found: " + String.Join<Process>(", ", foundProcesses));
        }
        var targetProcess = foundProcesses[0];

        if (WasInjectedLogicAlreadyInjectedIntoTheGameProcess(targetProcess)) return;

        // Ability to pass an argument as a structure
        //var arg = new Argument { myNum = 12345 };
        //var injectionResult = targetProcess.Inject(RuntimeConfigName, NameOfDllToInject, AssemblyQualifiedTypeName, NameOfEntryPointMethod, arg, true);
        
        var injectionResult = targetProcess.Inject(runtimeConfigName, nameOfDllToInject, assemblyQualifiedTypeName, nameOfEntryPointMethod, "", waitForReturnFromInjectedLogic);
        if (waitForReturnFromInjectedLogic == true)
        {
            if(injectionResult != 0) Console.WriteLine("An error occurred during injection, code: " + injectionResult);
            else Console.WriteLine("The injection was successful");
        }
        else Console.WriteLine("Injection completed, check out the new console");

    }
    
    struct Argument
    {
        public int myNum;
    }
    
    private static bool WasInjectedLogicAlreadyInjectedIntoTheGameProcess(Process gameProcess)
    {
        var modules = gameProcess.Modules;
        bool wasHostFxrFound = false;
        bool wasInjectedLogicDllFound = false;
        // Since "InjectedLogic" will be injected after the game starts, all .dlls associated with .Net Runtime will come last, so it’s better to start the loop from the end
        for (int i = modules.Count - 1; i >= 0; i--)
        {
            var moduleName = modules[i];
            if (moduleName?.ModuleName?.StartsWith(injectedLogicName) == true)
            {
                wasInjectedLogicDllFound = true;
                if (wasHostFxrFound) break;
            }

            if (moduleName?.ModuleName?.StartsWith(hostFxrModuleName) == true)
            {
                wasHostFxrFound = true;
                if (wasInjectedLogicDllFound) break;
            }
        }

        if (wasInjectedLogicDllFound)
        {
            if (wasHostFxrFound) return true;
            else throw new ApplicationException($"{injectedLogicName} project was injected, but {hostFxrModuleName} was not. This case is not possible");
        }

        if (wasHostFxrFound)
        {
            throw new ApplicationException($"{hostFxrModuleName} was injected, but {injectedLogicName} was not. This case is rare. There was probably an error during the injection. You need to restart the game and perform the injection again");
        }

        return false;

    }
}