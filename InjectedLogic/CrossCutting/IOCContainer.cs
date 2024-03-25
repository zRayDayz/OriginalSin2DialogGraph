namespace InjectedLogic;

public class IOCContainer
{
    public static SimpleIOCContainer Instance;
    
    public static void CreateIOCContainer()
    {
        var IOCContainer = new SimpleIOCContainer();
            
        IOCContainer.ArrayPoolKeeper = CreateThreadUnsafeLazyObj(() => new ArrayPoolKeeper<byte>());
        IOCContainer.NativeStringWrapperPool = CreateThreadUnsafeLazyObj(() => new NativeStringWrapperPool(128));

        IOCContainer.ConfigManager = CreateThreadUnsafeLazyObj(() => new ConfigManager());
        IOCContainer.HooksSetter = CreateThreadUnsafeLazyObj(() => new HooksSetter(IOCContainer.ConfigManager.Value, IOCContainer.ConsoleProcessor.Value));
        IOCContainer.DataSender = CreateThreadUnsafeLazyObj(() => new DataSender());
        IOCContainer.ServerManager = CreateThreadUnsafeLazyObj(() => new ServerManager(IOCContainer.Server));
        IOCContainer.Server = CreateThreadUnsafeLazyObj(() => new TCPServer(IOCContainer.ConfigManager.Value, IOCContainer.DataSender.Value, IOCContainer.ConsoleProcessor.Value, IOCContainer.ArrayPoolKeeper.Value));
        IOCContainer.ConsoleProcessor = CreateThreadUnsafeLazyObj(() => new ConsoleProcessor(IOCContainer.ConfigManager.Value, IOCContainer.ServerManager.Value));

        Instance = IOCContainer;
    }
    
    private static Lazy<T> CreateThreadUnsafeLazyObj<T>(Func<T> valueVactory)
    {
        return new Lazy<T>(valueVactory, LazyThreadSafetyMode.None);
    }
}

public class SimpleIOCContainer
{
    public Lazy<ArrayPoolKeeper<byte>> ArrayPoolKeeper;
    public Lazy<NativeStringWrapperPool> NativeStringWrapperPool;

    public Lazy<ConfigManager> ConfigManager;
    public Lazy<HooksSetter> HooksSetter;
    public Lazy<DataSender> DataSender;
    public Lazy<ServerManager> ServerManager;
    public Lazy<TCPServer> Server;
    public Lazy<ConsoleProcessor> ConsoleProcessor;
}