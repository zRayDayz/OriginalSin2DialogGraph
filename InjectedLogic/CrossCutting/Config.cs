using System.Text.Json.Serialization;

namespace InjectedLogic;

public class Config
{
    [JsonInclude] 
    public bool IsServerLogEnabled { get; private set; } = true;
    
    [JsonInclude] 
    public bool IsHooksLogEnabled { get; private set; } = true;
    
    [JsonInclude] 
    public bool IsHooksTrashLogEnabled { get; private set; } = true;

    [JsonInclude] 
    public bool IsServerAutoRestartEnabled { get; private set; } = true;

    [JsonInclude] 
    public int Port { get; private set; } = 15234;

    [JsonInclude] 
    public string Ip { get; private set; } = "127.0.0.1";
    
    [JsonInclude]
    public int ReadTimeout { get; private set; } = 5000;
    
    [JsonInclude]
    public int WriteTimeout { get; private set; } = 5000;

    [JsonInclude]
    public int ServerWaitTimeBeforeNextSend { get; private set; } = 0;
}