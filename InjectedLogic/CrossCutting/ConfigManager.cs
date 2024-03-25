using System.Text.Json;

namespace InjectedLogic;

public class ConfigManager
{
    protected const string modFolderDirectory = "DialogGraphMod";
    protected const string modConfigName = "modConfig.json";

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

    public virtual void InitializeConfig()
    {
        if (isInitialized) throw new InvalidOperationException("The config has already been initialized");

        string fullModDirectoryPath = GetModDirectoryPath();
        var fullConfigPath = fullModDirectoryPath + "\\" + modConfigName;
        if (Directory.Exists(fullModDirectoryPath) == false || File.Exists(fullConfigPath) == false)
        {
            Console.Error.WriteLine($"{Environment.NewLine}The config at the path: {fullConfigPath} - was not found{Environment.NewLine}");
            config = new Config();
        }
        else
        {
            var configText = File.ReadAllText(fullConfigPath);
            var parsedConfig = JsonSerializer.Deserialize<Config>(configText, jsonSerializerOptions);
            config = parsedConfig ?? new Config();
        }
        
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