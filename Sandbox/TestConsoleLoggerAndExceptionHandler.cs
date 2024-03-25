using System.Runtime.CompilerServices;
using System.Windows;
using DialogGraph;

namespace Sandbox;

public class TestConsoleLoggerAndExceptionHandler : ILogger, IExceptionHandler
{
    private const string exceptionMsgHeader = "++++++++++++++++ EXCEPTION ++++++++++++++++";
    private const string exceptionMsgFooter = "+++++++++++++++++++++++++++++++++++++++++++";
    private const string exceptionHeader = "EXCEPTION";
    
    private IMediator mediator;
    private bool IsInit;
    
    public TestConsoleLoggerAndExceptionHandler(IMediator mediator)
    {
        this.mediator = mediator;
    }
    
    public void Initialize()
    {
        IsInit = true;
    }

    public void LogText(string text)
    {
        Console.Write(text);
        if (IsInit) mediator?.Send(new LoggingMessage(text));
    }

    public void HandleExceptionBlockingWay(Exception e, object sender, [CallerMemberName] string callerMemberName = "")
    {
        string exceptionText = exceptionMsgHeader + Environment.NewLine + e + Environment.NewLine + exceptionMsgFooter + Environment.NewLine;
        LogText(exceptionText);
        MessageBox.Show(exceptionText, exceptionHeader);
    }
    
    public void HandleExceptionBlockingWay(string exceptionMessage, object sender, [CallerMemberName] string callerMemberName = "")
    {
        string exceptionText = exceptionMsgHeader + Environment.NewLine + exceptionMessage + Environment.NewLine + exceptionMsgFooter + Environment.NewLine;
        LogText(exceptionText);
        MessageBox.Show(exceptionText, exceptionHeader);
    }
    
    public void HandleExceptionNonBlockingWay(Exception e, object sender, [CallerMemberName] string callerMemberName = "")
    {
        string exceptionText = exceptionMsgHeader + Environment.NewLine + e + Environment.NewLine + exceptionMsgFooter + Environment.NewLine;
        LogText(exceptionText);
    }
    
    public void HandleExceptionNonBlockingWay(string exceptionMessage, object sender, [CallerMemberName] string callerMemberName = "")
    {
        string exceptionText = exceptionMsgHeader + Environment.NewLine + exceptionMessage + Environment.NewLine + exceptionMsgFooter + Environment.NewLine;
        LogText(exceptionText);
    }
}