using System.Windows;
using System.Windows.Threading;

namespace DialogGraph;

public class NetworkClientManager : ILogger, IExceptionHandler
{
    private INetworkClient client;
    private DataReceiverFactory dataReceiverFactory;
    private ILogger logger;
    private IExceptionHandler exceptionHandler;
    private Dispatcher dispatcher;
    public DataReceiver DataReceiver { get; private set; }

    // "volatile" operator does not provide thread safety, but it is not important here because simultaneously only one thread writes to these fields
    private volatile CancellationTokenSource cancellationTokenSource;
    private volatile Task clientTask;
    private volatile bool isClientRestarting;
    
    public NetworkClientManager(IMediator mediator, INetworkClient client, DataReceiverFactory dataReceiverFactory, ILogger logger, IExceptionHandler exceptionHandler)
    {
        this.client = client;
        this.dataReceiverFactory = dataReceiverFactory;
        this.logger = logger;
        this.exceptionHandler = exceptionHandler;
        dispatcher = Application.Current.Dispatcher;
        
        mediator.Register((TryRestartNetworkClientMessage arg) =>
        {
            bool result = TryRestartClient(out string textResult);
            return new NetworkClientMessageResult(result, textResult);
        });
        
        mediator.Register((TryStopNetworkClientMessage arg) =>
        {
            bool result = TryStopClient(out string textResult);
            return new NetworkClientMessageResult(result, textResult);
        });
    }

    public bool TryStartClient(out string textResult)
    {
        textResult = String.Empty;
        if (isClientRestarting)
        {
            textResult = "Client is restarting";
            return false;
        }
        
        if (clientTask?.IsCompleted == false)
        {
            textResult = "Client is running";
            return false;
        }

        StartClient();
        return true;
    }
    
    private void StartClient()
    {
        cancellationTokenSource?.Dispose();
        
        cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        
        var progress = new Progress<byte[]>();
        DataReceiver = dataReceiverFactory.Create(progress);
        clientTask = Task.Run(() => client.StartClientAsync(progress, this, this, cancellationToken));
    }
    
    public bool TryRestartClient(out string textResult)
    {
        textResult = String.Empty;
        if (isClientRestarting)
        {
            textResult = "Client is restarting";
            return false;
        }
        
        isClientRestarting = true;
        cancellationTokenSource.Cancel();

        clientTask.ContinueWith(task =>
        {
            StartClient();
            isClientRestarting = false;
        });

        return true;
    }
    
    public bool TryStopClient(out string textResult)
    {
        textResult = String.Empty;
        if (isClientRestarting)
        {
            textResult = "Client is restarting";
            return false;
        }
        
        if (clientTask?.IsCompleted is null or true)
        {
            textResult = "Client isn't running";
            return false;
        }

        cancellationTokenSource.Cancel();
        return true;
    }

    public void LogText(string text)
    {
        dispatcher.BeginInvoke(logger.LogText, text);
    }
    
    public void Initialize() => throw new NotSupportedException();
    
    public void HandleExceptionNonBlockingWay(string exceptionMessage, object sender, string callerMemberName = "")
    {
        dispatcher.BeginInvoke((string exceptionMessage, object sender, string callerMemberName) => exceptionHandler.HandleExceptionNonBlockingWay(exceptionMessage, sender, callerMemberName), exceptionMessage, sender, callerMemberName);
    }
    
    public void HandleExceptionBlockingWay(Exception e, object sender, string callerMemberName = "")  => throw new NotSupportedException();
    public void HandleExceptionBlockingWay(string exceptionMessage, object sender, string callerMemberName = "")  => throw new NotSupportedException();
    public void HandleExceptionNonBlockingWay(Exception e, object sender, string callerMemberName = "") => throw new NotSupportedException();
    
    
}