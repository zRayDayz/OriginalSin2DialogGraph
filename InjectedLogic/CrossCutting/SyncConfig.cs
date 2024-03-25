namespace InjectedLogic;

public class SyncConfig
{
    public SyncConfig(Config config)
    {
        IsServerLogEnabled = config.IsServerLogEnabled;
        IsServerAutoRestartEnabled = config.IsServerAutoRestartEnabled;
        IsHooksLogEnabled = config.IsHooksLogEnabled;
        IsHooksTrashLogEnabled = config.IsHooksTrashLogEnabled;
    }
    
    public readonly object ConsoleLock = new object();
    
    // "volatile" operator does not provide thread safety, but it is not important here because only one thread writes to this field
    private volatile bool isServerLogEnabled;
    public bool IsServerLogEnabled
    {
        get => isServerLogEnabled; 
        set => isServerLogEnabled = value;
    }
    
    private volatile bool isServerAutoRestartEnabled;
    public bool IsServerAutoRestartEnabled
    {
        get => isServerAutoRestartEnabled; 
        set => isServerAutoRestartEnabled = value;
    }
    
    private volatile bool isHooksLogEnabled;
    public bool IsHooksLogEnabled
    {
        get => isHooksLogEnabled; 
        set => isHooksLogEnabled = value;
    }
    
    private volatile bool isHooksTrashLogEnabled;
    public bool IsHooksTrashLogEnabled
    {
        get => isHooksTrashLogEnabled; 
        set => isHooksTrashLogEnabled = value;
    }
}