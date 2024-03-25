using System.IO;
using System.Text.Json;

namespace DialogGraph;

public class ConfigManager
{
    protected const string modFolderDirectory = "DialogGraphMod";
    protected const string modConfigName = "config.json";
    protected const string modDialogFolderName = "Dialogs";

    protected readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
    protected Config config;
    public virtual Config Config
    {
        get
        {
            if (isInitialized == false) throw new InvalidOperationException("Trying to get a config before it is initialized");
            return config;
        }
    }
    
    protected SyncConfig syncConfig;
    public virtual SyncConfig SyncConfig
    {
        get
        {
            if (isInitialized == false) throw new InvalidOperationException("Trying to get a config before it is initialized");
            return syncConfig;
        }
    }

    protected bool isInitialized;
    protected ILogger logger;

    public ConfigManager(ILogger logger)
    {
        this.logger = logger;
    }
    
    public virtual void InitializeConfig()
    {
        if (isInitialized) throw new InvalidOperationException("The config has already been initialized");
            
        string fullModDirectoryPath = GetModDirectoryPath();
        var fullConfigPath = fullModDirectoryPath + "\\" + modConfigName;
        if (Directory.Exists(fullModDirectoryPath) == false || File.Exists(fullConfigPath) == false)
        {
            logger.LogText($"{Environment.NewLine}========== CONFIG MANAGER =========={Environment.NewLine}" +
                           $"The config at the path: {fullConfigPath} - was not found{Environment.NewLine}" +
                           $"=========={Environment.NewLine}");
            config = new Config();
            this.isInitialized = true;
        }
        else
        {
            var configText = File.ReadAllText(fullConfigPath);
            var parsedConfig = JsonSerializer.Deserialize<Config>(configText, jsonSerializerOptions);
            config = parsedConfig ?? new Config();
        }

        config.DialogFolderPath = fullModDirectoryPath + "\\" + modDialogFolderName;
        if (Directory.Exists(config.DialogFolderPath) == false) throw new ApplicationException($"{Environment.NewLine}========== CONFIG MANAGER =========={Environment.NewLine}" +
                                                                              $"The dialog folder at the path: {config.DialogFolderPath} - was not found{Environment.NewLine}" +
                                                                              $"=========={Environment.NewLine}");
        
        config.LocalizationFileFullPath = fullModDirectoryPath + "\\" + config.LocalizationFileName;
        
        syncConfig = new SyncConfig(config);
        
        this.isInitialized = true;
    }
    
    protected virtual string GetModDirectoryPath()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        var fullModDirectoryPath = currentDirectory + "\\" + modFolderDirectory;
        return fullModDirectoryPath;
    }
}