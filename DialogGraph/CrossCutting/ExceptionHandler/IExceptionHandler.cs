using System.Runtime.CompilerServices;

namespace DialogGraph;

public interface IExceptionHandler
{
    public void HandleExceptionBlockingWay(Exception e, object sender, [CallerMemberName] string callerMemberName = "");

    public void HandleExceptionBlockingWay(string exceptionMessage, object sender, [CallerMemberName] string callerMemberName = "");

    public void HandleExceptionNonBlockingWay(Exception e, object sender, [CallerMemberName] string callerMemberName = "");

    public void HandleExceptionNonBlockingWay(string exceptionMessage, object sender, [CallerMemberName] string callerMemberName = "");
}