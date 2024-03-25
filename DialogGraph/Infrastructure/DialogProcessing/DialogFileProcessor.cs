using System.IO;

namespace DialogGraph;

public class DialogFileProcessor : IDialogFileProcessor
{
    private string dialogFolderPath;
    private DialogJsonParser dialogJsonParser;
    public DialogFileProcessor(ConfigManager configManager, DialogJsonParser dialogJsonParser)
    {
        var config = configManager.Config;
        dialogFolderPath = config.DialogFolderPath;
        this.dialogJsonParser = dialogJsonParser;
    }
    
    public Dialog CreateNewDialogByName(string dialogName)
    {
        using var stream = GetDialogFileStream(dialogName);
        if (stream == null) throw new InvalidOperationException("Dialogue file was not found, Stream was not created");
        return dialogJsonParser.ParseDialogFile(dialogName, stream);
    }
    
    public StreamReader GetDialogFileStream(string dialogName)
    {
        string[] files = Directory.GetFiles(dialogFolderPath, $"{dialogName}.*", SearchOption.AllDirectories);
        if (files.Length == 0) return null;
        string dialogFilePath = files[0];
        return new StreamReader(dialogFilePath);
    }
}