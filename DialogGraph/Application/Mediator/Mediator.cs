using System.Runtime.CompilerServices;

namespace DialogGraph;

// For each Message, only one single Handler is allowed, just like in the MediatR library
// This class additionally allows to use structures (without boxing)
public class SimpleMediator : IMediator
{
    private readonly Dictionary<Type, CallbackHolder> dependencies = new();
    public void Register<TMessage>(Action<TMessage> callback) where TMessage : IMediatorMessage
    {
        var messageType = typeof(TMessage);
        RegisterInternal(messageType, callback);
    }

    public void Register<TMessage, TResult>(Func<TMessage, TResult> callback) where TMessage : IMediatorMessage<TResult>
    {
        var messageType = typeof(TMessage);
        RegisterInternal(messageType, callback);
    }

    private void RegisterInternal(Type messageType, Delegate commonCallback)
    {
        if (dependencies.TryGetValue(messageType, out CallbackHolder foundCallback))
        {
            throw new ArgumentException($"A handler for {messageType.FullName} message type has already been registered. Type name: {foundCallback.callback.Target.GetType().FullName}; Method name: {foundCallback.callback.Method.Name}");
        }

        dependencies.Add(messageType, new CallbackHolder(commonCallback));
    }

    public void Send<TMessage>(TMessage message) where TMessage : IMediatorMessage
    {
        var messageType = typeof(TMessage);
        if (dependencies.TryGetValue(messageType, out var callbackHolder))
        {
            var callback = callbackHolder.callback as Action<TMessage>;
            callback(message);
        }
        else
        {
            throw new ArgumentException($"No registered callbacks were found for type {messageType.FullName}");
        }
    }

    // Unlike "public TResult Send<TResult>(IMediatorMessage<TResult> message)", it requires explicitly specifying Generic parameters when calling, but allows throwing structure arguments without boxing
    public TResult Send<TMessage,TResult>(TMessage message) where TMessage : IMediatorMessage<TResult>
    {
        var messageType = typeof(TMessage);
        if (dependencies.TryGetValue(messageType, out var callbackHolder))
        {
            var callback = callbackHolder.callback as Func<TMessage, TResult>;
            return callback(message);
        }
        else
        {
            throw new ArgumentException($"No registered callbacks were found for type {messageType.FullName}");
        }
    }
}

struct CallbackHolder
{
    public CallbackHolder(Delegate callback)
    {
        this.callback = callback;
    }
    public Delegate callback;
}
