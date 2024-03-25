using System.Windows;
using DialogGraph;

namespace Sandbox;

public class ServerConsoleWPFTest
{
    private async Task StartServerAndConsole()
    {
        TestIOCContainer.CreateIOCContainer();
        TestIOCContainer.Instance.InjectedLogicConfigManager.InitializeConfig();
        TestIOCContainer.Instance.DialogGraphConfigManager.Value.InitializeConfig();
        
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var serverTask = Task.Factory.StartNew(() => TestIOCContainer.Instance.Server.Value.StartServer(cancellationToken), TaskCreationOptions.LongRunning);
        
        await Task.Factory.StartNew(() => TestIOCContainer.Instance.ConsoleProcessor.Value.RunConsole(), TaskCreationOptions.LongRunning);

        cancellationTokenSource.Cancel();
    }

    public void Start()
    {
        Task serverTask = StartServerAndConsole();
        
        Application wpfApp = new App();
        wpfApp.Run();

        serverTask.Wait();
    }
}