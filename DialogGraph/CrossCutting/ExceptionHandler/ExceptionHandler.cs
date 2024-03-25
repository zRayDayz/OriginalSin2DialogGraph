using System.Runtime.CompilerServices;
using System.Windows;

namespace DialogGraph;

public class ExceptionHandler : IExceptionHandler
{
    private ILogger logger;
    private const string exceptionMsgHeader = "++++++++++++++++ EXCEPTION ++++++++++++++++";
    private const string exceptionMsgFooter = "+++++++++++++++++++++++++++++++++++++++++++";
    private const string exceptionHeader = "EXCEPTION";

    public ExceptionHandler(ILogger logger)
    {
        this.logger = logger;
    }
    
    public void HandleExceptionBlockingWay(Exception e, object sender, [CallerMemberName] string callerMemberName = "")
    {
        string exceptionText = exceptionMsgHeader + Environment.NewLine + e + Environment.NewLine + exceptionMsgFooter + Environment.NewLine;
        logger.LogText(exceptionText);
        MessageBox.Show(exceptionText, exceptionHeader);
    }
    
    public void HandleExceptionBlockingWay(string exceptionMessage, object sender, [CallerMemberName] string callerMemberName = "")
    {
        string exceptionText = exceptionMsgHeader + Environment.NewLine + exceptionMessage + Environment.NewLine + exceptionMsgFooter + Environment.NewLine;
        logger.LogText(exceptionText);
        MessageBox.Show(exceptionText, exceptionHeader);
    }
    
    public void HandleExceptionNonBlockingWay(Exception e, object sender, [CallerMemberName] string callerMemberName = "")
    {
        string exceptionText = exceptionMsgHeader + Environment.NewLine + e + Environment.NewLine + exceptionMsgFooter + Environment.NewLine;
        logger.LogText(exceptionText);
    }
    
    public void HandleExceptionNonBlockingWay(string exceptionMessage, object sender, [CallerMemberName] string callerMemberName = "")
    {
        string exceptionText = exceptionMsgHeader + Environment.NewLine + exceptionMessage + Environment.NewLine + exceptionMsgFooter + Environment.NewLine;
        logger.LogText(exceptionText);
    }
}