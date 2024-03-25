namespace DialogGraph;

public class AppInitializer
{
    private static AppInitializer instance;
    public static AppInitializer Instance
    {
        get => instance ??= new AppInitializer();
        set => instance = value;
    }
    
    protected virtual void CreateIOCContainer()
    {
        IOCContainer.CreateIOCContainer();
    }
    
    // The order in which functions are called (services run) is important because some services depend on others. However, the order of their initialization (objects creation, i.e. constructors calls) is not important
    public virtual void InitializeApp()
    {
        CreateIOCContainer();
            
        IOCContainer.Instance.ConfigManager.Value.InitializeConfig();
            
        IOCContainer.Instance.MainWindow.Value.Show();
            
        _ = IOCContainer.Instance.CustomGraphProcessor.Value;
            
        IOCContainer.Instance.NetworkClientManager.Value.TryStartClient(out string _);
            
        IOCContainer.Instance.Logger.Value.Initialize();
    }
}