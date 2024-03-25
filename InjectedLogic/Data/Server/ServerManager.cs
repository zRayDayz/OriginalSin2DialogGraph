namespace InjectedLogic;

public class ServerManager
{
    private Lazy<TCPServer> serverLazy;
    // "volatile" operator does not provide thread safety, but it is not important here because simultaneously only one thread writes to these fields
    private volatile CancellationTokenSource cancellationTokenSource;
    private volatile Task serverTask;
    private volatile bool isServerRestarting;

    public ServerManager(Lazy<TCPServer> serverLazy)
    {
        this.serverLazy = serverLazy;
    }
    
    public bool TryStartServer(out string textResult)
    {
        textResult = String.Empty;
        if (isServerRestarting)
        {
            textResult = "Server is restarting";
            return false;
        }
        
        if (serverTask?.IsCompleted == false)
        {
            textResult = "Server is running";
            return false;
        }

        StartServer();
        return true;
    }
    
    private void StartServer()
    {
        cancellationTokenSource?.Dispose();
        
        cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        TCPServer server = serverLazy.Value;
        serverTask = Task.Factory.StartNew(() => server.StartServer(cancellationToken), TaskCreationOptions.LongRunning);
    }
    
    public bool TryRestartServer(out string textResult)
    {
        textResult = String.Empty;
        if (isServerRestarting)
        {
            textResult = "Server is restarting";
            return false;
        }
        
        isServerRestarting = true;
        cancellationTokenSource.Cancel();

        serverTask.ContinueWith(task =>
        {
            StartServer();
            isServerRestarting = false;
        });

        return true;
    }
    
    public bool TryStopServer(out string textResult)
    {
        textResult = String.Empty;
        if (isServerRestarting)
        {
            textResult = "Server is restarting";
            return false;
        }
        
        if (serverTask?.IsCompleted is null or true)
        {
            textResult = "Server isn't running";
            return false;
        }

        cancellationTokenSource.Cancel();
        return true;
    }
}