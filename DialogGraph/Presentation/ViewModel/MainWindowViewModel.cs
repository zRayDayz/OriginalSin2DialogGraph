using System.Windows.Input;
using DialogGraph.Commands;
using GraphShape.Algorithms.Layout;
using GraphShape.Utils;

namespace DialogGraph;

public class MainWindowViewModel : NotifierObject
{
    private IMediator mediator;
    private IMainWindowActions mainWindowActions;
    private SyncConfig syncConfig;
    
    public MainWindowViewModel(IMediator mediator, ConfigManager configManager, IMainWindowActions mainWindowActions, ICustomHighlightAlgorithmFactoryProvider customHighlightAlgorithmFactoryProvider)
    {
        this.mediator = mediator;
        this.mainWindowActions = mainWindowActions;
        syncConfig = configManager.SyncConfig;
        isTCPClientLogEnabled = syncConfig.IsNetworkClientLogEnabled;
        
        mediator.Register<LoggingMessage>(LogText);

        TryRestartClientCommand = new RelayCommand(TryRestartClient);
        TryStopClientCommand = new RelayCommand(TryStopClient);
        
        LayoutAlgorithmType = "Sugiyama";
        LayoutParameters = new SugiyamaLayoutParameters() { LayerGap = 50, SliceGap = 50 };
        HighlightAlgorithmFactory = customHighlightAlgorithmFactoryProvider.CreateFactory();
    }
    
    private CustomGraph graph;
    public CustomGraph Graph
    {
        get => graph;
        set
        {
            if (graph == value)
                return;

            graph = value;
            OnPropertyChanged();
        }
    }

    public string LayoutAlgorithmType { get; }
    
    public CustomHighlightAlgorithmFactory HighlightAlgorithmFactory { get; }

    private ILayoutParameters layoutParameters;
    public ILayoutParameters LayoutParameters
    {
        get => layoutParameters;
        set
        {
            if (layoutParameters != null && layoutParameters.Equals(value))
                return;
            
            layoutParameters = value;
            OnPropertyChanged();
        }
    }

    public void LogText(LoggingMessage message)
    {
        mainWindowActions.AppendTextToLog(message.text);
    }

    private bool isTCPClientLogEnabled;
    public bool IsTCPClientLogEnabled
    {
        get => isTCPClientLogEnabled;
        set
        {
            isTCPClientLogEnabled = value;
            syncConfig.IsNetworkClientLogEnabled = value;
        }
    }

    public ICommand TryRestartClientCommand { get; }
    private void TryRestartClient(object obj)
    {
        var packedResult = mediator.Send<TryRestartNetworkClientMessage, NetworkClientMessageResult>(new TryRestartNetworkClientMessage());
        if (packedResult.result == false) LogText(new LoggingMessage($"Unable to restart the client. Reason: {packedResult.textResult}" + Environment.NewLine));
        else LogText(new LoggingMessage("The client is restarting" + Environment.NewLine));
    }

    public ICommand TryStopClientCommand { get; }
    private void TryStopClient(object obj)
    {
        var packedResult = mediator.Send<TryStopNetworkClientMessage, NetworkClientMessageResult>(new TryStopNetworkClientMessage());
        if (packedResult.result == false) LogText(new LoggingMessage($"Unable to stop the client. Reason: {packedResult.textResult}" + Environment.NewLine));
        else LogText(new LoggingMessage("The client is stopping" + Environment.NewLine));
    }
}