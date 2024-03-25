namespace DialogGraph;

public class UILogger : ILogger
{
    private static List<string> messages = new List<string>();
    private IMediator mediator;
    private bool IsInit;
    
    public UILogger(IMediator mediator)
    {
        this.mediator = mediator;
    }
    
    public void Initialize()
    {
        mediator.Send(new LoggingMessage(String.Join("", messages)));
        messages = null;
        IsInit = true;
    }

    public void LogText(string text)
    {
        if (IsInit) mediator.Send(new LoggingMessage(text));
        else messages.Add(text);
    }
}