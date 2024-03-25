namespace Sandbox;

public class ServerClientConsoleTest
{
    public void StartServerClientAndConsole()
    {
        TestIOCContainer.CreateIOCContainer();
        TestIOCContainer.Instance.InjectedLogicConfigManager.InitializeConfig();
        TestIOCContainer.Instance.DialogGraphConfigManager.Value.InitializeConfig();
        
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var serverTask = Task.Factory.StartNew(() => TestIOCContainer.Instance.Server.Value.StartServer(cancellationToken), TaskCreationOptions.LongRunning);
        
        var progress = new Progress<byte[]>(arr => TestIOCContainer.Instance.DataReceiver.ReceiveDataBytes(arr));
        TestIOCContainer.Instance.NetworkClient.Value.StartClientAsync(progress, TestIOCContainer.Instance.Logger.Value, TestIOCContainer.Instance.ExceptionHandler.Value, cancellationToken);

        TestIOCContainer.Instance.ConsoleProcessor.Value.RunConsole();
            
        cancellationTokenSource.Cancel();
    }

    public void StartServerAndConsole()
    {
        TestIOCContainer.CreateIOCContainer();
        TestIOCContainer.Instance.InjectedLogicConfigManager.InitializeConfig();
        TestIOCContainer.Instance.DialogGraphConfigManager.Value.InitializeConfig();
        
        
        TestIOCContainer.Instance.ServerManager.Value.TryStartServer(out string _);

        TestIOCContainer.Instance.ConsoleProcessor.Value.RunConsole();
            
        TestIOCContainer.Instance.ServerManager.Value.TryStopServer(out string _);
        
        
        // var cancellationTokenSource = new CancellationTokenSource();
        // var cancellationToken = cancellationTokenSource.Token;
        // var serverTask = Task.Factory.StartNew(() => TestIOCContainer.Instance.Server.Value.StartServer(cancellationToken), TaskCreationOptions.LongRunning);
        //
        // TestIOCContainer.Instance.ConsoleProcessor.Value.RunConsole();
        //     
        // cancellationTokenSource.Cancel();
    }
}