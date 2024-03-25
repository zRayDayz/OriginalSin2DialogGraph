namespace DialogGraph;

public struct LoggingMessage : IMediatorMessage
{
    public string text;

    public LoggingMessage(string text)
    {
        this.text = text;
    }
}