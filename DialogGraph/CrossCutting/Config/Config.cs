using System.Text.Json.Serialization;

namespace DialogGraph;

public class Config
{
    [JsonInclude] 
    public bool IsNetworkClientLogEnabled { get; private set; } = true;
    
    [JsonInclude] 
    public bool IsNetworkClientAutoRestartEnabled { get; private set; } = true;

    [JsonInclude] 
    public int Port { get; private set; } = 15234;

    [JsonInclude] 
    public string Ip { get; private set; } = "127.0.0.1";
    
    [JsonInclude] 
    public int MaximumNumberOfAttemptsToConnect { get; private set; } = -1;
    
    [JsonInclude]
    public int ReadTimeout { get; private set; } = 5000;
    
    [JsonInclude]
    public int WriteTimeout { get; private set; } = 5000;

    [JsonInclude] 
    public string LocalizationFileName { get; private set; } = "russian.xml";

    public string LocalizationFileFullPath { get; set; }

    public string DialogFolderPath { get; set; }

}