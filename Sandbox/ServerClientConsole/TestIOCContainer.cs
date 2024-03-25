using DialogGraph;
using InjectedLogic;

namespace Sandbox;

public class TestIOCContainer
{
    public static SimpleIOCContainer Instance;
    
    public static void CreateIOCContainer()
    {
        var IOCContainer = new SimpleIOCContainer();
        
        #region InjectedLogic
        IOCContainer.ArrayPoolKeeper = new ArrayPoolKeeper<byte>();
        IOCContainer.InjectedLogicConfigManager = new InjectedLogic.ConfigManager();
        
        IOCContainer.TestDataSender = new TestDataSender();
        IOCContainer.ServerManager = CreateThreadUnsafeLazyObj(() => new ServerManager(IOCContainer.Server));
        IOCContainer.Server = CreateThreadUnsafeLazyObj(() => new TCPServer(IOCContainer.InjectedLogicConfigManager, IOCContainer.TestDataSender, IOCContainer.ConsoleProcessor.Value, IOCContainer.ArrayPoolKeeper));
        IOCContainer.ConsoleProcessor = CreateThreadUnsafeLazyObj(() => new ConsoleProcessor(IOCContainer.InjectedLogicConfigManager, IOCContainer.ServerManager.Value));
        #endregion

        #region DialogGraph
        DialogGraph.IOCContainer.CreateIOCContainer();
        DialogGraph.IOCContainer.Instance.Logger = CreateThreadUnsafeLazyObj(() => IOCContainer.Logger.Value);
        DialogGraph.IOCContainer.Instance.ExceptionHandler = CreateThreadUnsafeLazyObj(() => (IExceptionHandler)IOCContainer.Logger.Value);
        DialogGraph.IOCContainer.Instance.ConfigManager = CreateThreadUnsafeLazyObj(() => IOCContainer.DialogGraphConfigManager.Value);;

        var logger = CreateThreadUnsafeLazyObj(() => new TestConsoleLoggerAndExceptionHandler(DialogGraph.IOCContainer.Instance.Mediator.Value));
        IOCContainer.Logger = CreateThreadUnsafeLazyObj(() => (ILogger)logger.Value);
        IOCContainer.ExceptionHandler = CreateThreadUnsafeLazyObj(() => (IExceptionHandler)logger.Value);
        IOCContainer.DialogGraphConfigManager = CreateThreadUnsafeLazyObj(() => (DialogGraph.ConfigManager)new TestDialogGraphConfigManager(IOCContainer.Logger.Value));
        IOCContainer.NetworkClient = DialogGraph.IOCContainer.Instance.NetworkClient;
        IOCContainer.DataReceiver = new TestDataReceiver();
        #endregion

        Instance = IOCContainer;
    }
    
    private static Lazy<T> CreateThreadUnsafeLazyObj<T>(Func<T> valueVactory)
    {
        return new Lazy<T>(valueVactory, LazyThreadSafetyMode.None);
    }
}

public class SimpleIOCContainer
{
    public ArrayPoolKeeper<byte> ArrayPoolKeeper;

    public InjectedLogic.ConfigManager InjectedLogicConfigManager;
    public TestDataSender TestDataSender;
    public Lazy<ServerManager> ServerManager;
    public Lazy<TCPServer> Server;
    public Lazy<ConsoleProcessor> ConsoleProcessor;
    
    public Lazy<DialogGraph.ConfigManager> DialogGraphConfigManager;
    public Lazy<ILogger> Logger;
    public Lazy<IExceptionHandler> ExceptionHandler;
    public Lazy<INetworkClient> NetworkClient;
    public TestDataReceiver DataReceiver;
    
}