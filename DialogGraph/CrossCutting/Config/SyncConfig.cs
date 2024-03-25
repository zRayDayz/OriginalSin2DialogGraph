namespace DialogGraph;

public class SyncConfig
{
    public SyncConfig(Config config)
    {
        IsNetworkClientLogEnabled = config.IsNetworkClientLogEnabled;
        IsNetworkClientAutoRestartEnabled = config.IsNetworkClientAutoRestartEnabled;
    }
    
    // "volatile" operator does not provide thread safety, but it is not important here because only one thread writes to this field
    private volatile bool isNetworkClientLogEnabled;
    public bool IsNetworkClientLogEnabled
    {
        get => isNetworkClientLogEnabled;
        set => isNetworkClientLogEnabled = value;
    }
    
    private volatile bool isNetworkClientAutoRestartEnabled;
    public bool IsNetworkClientAutoRestartEnabled
    {
        get => isNetworkClientAutoRestartEnabled; 
        set => isNetworkClientAutoRestartEnabled = value;
    }
}