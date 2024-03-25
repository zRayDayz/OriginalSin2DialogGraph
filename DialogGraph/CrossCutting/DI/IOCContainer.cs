namespace DialogGraph;

public class IOCContainer
{
    public static SimpleIOCContainer Instance;

    public static void CreateIOCContainer()
    {
        var IOCContainer = new SimpleIOCContainer();

        #region Shared
        IOCContainer.Mediator = CreateThreadUnsafeLazyObj(() => (IMediator)new SimpleMediator());
        IOCContainer.Logger = CreateThreadUnsafeLazyObj(() => (ILogger)new UILogger(IOCContainer.Mediator.Value));
        IOCContainer.ConfigManager = CreateThreadUnsafeLazyObj(() => new ConfigManager(IOCContainer.Logger.Value));
        IOCContainer.ExceptionHandler = CreateThreadUnsafeLazyObj(() => (IExceptionHandler)new ExceptionHandler(IOCContainer.Logger.Value));
        #endregion

        #region Presentation
        IOCContainer.CustomHighlightAlgorithmFactoryProvider = CreateThreadUnsafeLazyObj(() => (ICustomHighlightAlgorithmFactoryProvider)new CustomHighlightAlgorithmFactoryProvider());
        IOCContainer.GraphLayoutFactory = CreateThreadUnsafeLazyObj(() => (IGraphLayoutFactory)new GraphLayoutFactory());
        IOCContainer.MainWindowActions = CreateThreadUnsafeLazyObj(() => (IMainWindowActions)new MainWindow.MainWindowActions());
        IOCContainer.CustomGraphProcessor = CreateThreadUnsafeLazyObj(() => new CustomGraphProcessor(IOCContainer.Mediator.Value, IOCContainer.MainWindowViewModel.Value));
        IOCContainer.MainWindowViewModel = CreateThreadUnsafeLazyObj(() => new MainWindowViewModel(IOCContainer.Mediator.Value, IOCContainer.ConfigManager.Value, IOCContainer.MainWindowActions.Value, IOCContainer.CustomHighlightAlgorithmFactoryProvider.Value));
        IOCContainer.MainWindow = CreateThreadUnsafeLazyObj(() => new MainWindow(IOCContainer.MainWindowViewModel.Value));
        #endregion
        
        #region NetworkClient and Application
        IOCContainer.NetworkClientManager = CreateThreadUnsafeLazyObj(() => new NetworkClientManager(IOCContainer.Mediator.Value, IOCContainer.NetworkClient.Value, IOCContainer.DataReceiverFactory.Value, IOCContainer.Logger.Value, IOCContainer.ExceptionHandler.Value));
        IOCContainer.LocalizationFileParser =  CreateThreadUnsafeLazyObj(() => (ILocalizationFileParser)new LocalizationFileParser(IOCContainer.ConfigManager.Value));
        IOCContainer.LocalizationFileProcessor =  CreateThreadUnsafeLazyObj(() => new LocalizationProcessor(IOCContainer.LocalizationFileParser.Value));
        IOCContainer.DialogFileProcessor = CreateThreadUnsafeLazyObj(() => (IDialogFileProcessor)new DialogFileProcessor(IOCContainer.ConfigManager.Value, IOCContainer.DialogGraphParser.Value));
        IOCContainer.DialogGraphParser = CreateThreadUnsafeLazyObj(() => new DialogJsonParser());
        IOCContainer.DialogProcessor = CreateThreadUnsafeLazyObj(() => (IDialogProcessor)new DialogProcessor(IOCContainer.Mediator.Value, IOCContainer.Logger.Value, IOCContainer.DialogFileProcessor.Value, IOCContainer.LocalizationFileProcessor.Value));
        IOCContainer.DataReceiverFactory = CreateThreadUnsafeLazyObj(() => new DataReceiverFactory(IOCContainer.DialogProcessor.Value, IOCContainer.ExceptionHandler.Value));
        IOCContainer.NetworkClient = CreateThreadUnsafeLazyObj(() => (INetworkClient)new TCPClient(IOCContainer.ConfigManager.Value));
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
    public Lazy<IMediator> Mediator;
    public Lazy<ILogger> Logger;
    public Lazy<ConfigManager> ConfigManager;
    public Lazy<IExceptionHandler> ExceptionHandler;

    public Lazy<ICustomHighlightAlgorithmFactoryProvider> CustomHighlightAlgorithmFactoryProvider;
    public Lazy<IGraphLayoutFactory> GraphLayoutFactory;
    public Lazy<IMainWindowActions> MainWindowActions;
    public Lazy<CustomGraphProcessor> CustomGraphProcessor;
    public Lazy<MainWindowViewModel> MainWindowViewModel;
    public Lazy<MainWindow> MainWindow;

    public Lazy<NetworkClientManager> NetworkClientManager;
    public Lazy<ILocalizationFileParser> LocalizationFileParser;
    public Lazy<LocalizationProcessor> LocalizationFileProcessor;
    public Lazy<IDialogFileProcessor> DialogFileProcessor;
    public Lazy<DialogJsonParser> DialogGraphParser;
    public Lazy<IDialogProcessor> DialogProcessor;
    public Lazy<DataReceiverFactory> DataReceiverFactory;
    public Lazy<INetworkClient> NetworkClient;
}


