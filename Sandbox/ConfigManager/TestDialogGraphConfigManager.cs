using DialogGraph;

namespace Sandbox;

public class TestDialogGraphConfigManager : ConfigManager
{
    private const string nameOfOutputFolderForBinaries = "bin_solution";
    
    public TestDialogGraphConfigManager(ILogger logger) : base(logger)
    {
    }

    // Use configs and dialogs from 
    protected override string GetModDirectoryPath()
    {
        string projectName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        
        string currentDirectory = Directory.GetCurrentDirectory();
        DirectoryInfo currentDirInfo = new DirectoryInfo(currentDirectory);
        DirectoryInfo parentDirInfo = currentDirInfo.Parent;
        
        while (parentDirInfo != null && parentDirInfo.Name != projectName)
        {
            parentDirInfo = parentDirInfo.Parent;
        }

        if (parentDirInfo == null) throw new ApplicationException("Could not find project directory");
        
        DirectoryInfo solutionFolder = parentDirInfo.Parent;
        DirectoryInfo outputFolderForBinaries = solutionFolder.GetDirectories(nameOfOutputFolderForBinaries, SearchOption.TopDirectoryOnly)[0];
        DirectoryInfo modFolder = outputFolderForBinaries.GetDirectories(modFolderDirectory, SearchOption.AllDirectories)[0];
        
        var fullModDirectoryPath = modFolder.FullName;
        return fullModDirectoryPath;
    }
}