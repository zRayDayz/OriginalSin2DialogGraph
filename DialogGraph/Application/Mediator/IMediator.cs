namespace DialogGraph;

public interface IMediator
{
    public void Register<TMessage>(Action<TMessage> callback) where TMessage : IMediatorMessage;
    public void Register<TMessage, TResult>(Func<TMessage, TResult> callback) where TMessage : IMediatorMessage<TResult>;
    public void Send<TMessage>(TMessage message) where TMessage : IMediatorMessage;
    public TResult Send<TMessage, TResult>(TMessage message) where TMessage : IMediatorMessage<TResult>;
}