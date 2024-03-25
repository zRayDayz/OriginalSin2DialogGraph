namespace InjectedLogic;

public class ConsoleProcessor : IIOProvider
{
    private SyncConfig syncConfig;
    private ServerManager serverManager;
    readonly AutoResetEvent are = new AutoResetEvent(false);

    private const string cmdToggleServerLog = "toggle_server_log";
    private const string cmdToggleServerAutoRestart = "toggle_auto_restart";
    private const string cmdToggleHooksLog = "toggle_hooks_log";
    private const string cmdToggleHooksTrashLog = "toggle_hooks_trash_log";
    private const string cmdRestartServer = "restart_server";
    private const string cmdStopServer = "stop_server";

    public ConsoleProcessor(ConfigManager configManager, ServerManager serverManager)
    {
        syncConfig = configManager.SyncConfig;
        this.serverManager = serverManager;
        Console.CancelKeyPress += (sender, args) =>
        {
            args.Cancel = true;
            are.Set();
        };
    }

    public void RunConsole()
    {
        WinAPIWrapper.QuickEditMode(false);
        PrintThatLogModeIsRunning();
            
        while (true)
        {
            are.WaitOne();
            
            // It is critical to use "lock" for Console.ReadLine() because ReadLine() blocks the console, including the Console.WriteLine() method. Until the user presses "Enter" for example, Console.WriteLine() will just wait
            // With "lock" it is possible to avoid such situation
            lock (syncConfig.ConsoleLock)
            {
                WinAPIWrapper.QuickEditMode(true);
                
                Console.WriteLine("=== COMMAND MODE ===");
                Console.WriteLine("Enter the command: ");
                Console.WriteLine($"1) {cmdToggleServerLog}");
                Console.WriteLine($"2) {cmdToggleServerAutoRestart}");
                Console.WriteLine($"3) {cmdToggleHooksLog}");
                Console.WriteLine($"4) {cmdToggleHooksTrashLog}");
                Console.WriteLine($"5) {cmdRestartServer}");
                Console.WriteLine($"6) {cmdStopServer}");
                Console.WriteLine("Or Press 'ENTER' to return to Log Mode");
                
                var enteredCommand = Console.ReadLine();
                bool result;
                string textResult;
                switch (enteredCommand)
                {
                    case cmdToggleServerLog:
                        syncConfig.IsServerLogEnabled = !syncConfig.IsServerLogEnabled;
                        Console.WriteLine($"{nameof(syncConfig.IsServerLogEnabled)} was set to {syncConfig.IsServerLogEnabled}");
                        break;
                    case cmdToggleServerAutoRestart:
                        syncConfig.IsServerAutoRestartEnabled = !syncConfig.IsServerAutoRestartEnabled;
                        Console.WriteLine($"{nameof(syncConfig.IsServerAutoRestartEnabled)} was set to {syncConfig.IsServerAutoRestartEnabled}");
                        break;
                    case cmdToggleHooksLog:
                        syncConfig.IsHooksLogEnabled = !syncConfig.IsHooksLogEnabled;
                        Console.WriteLine($"{nameof(syncConfig.IsHooksLogEnabled)} was set to {syncConfig.IsHooksLogEnabled}");
                        break;
                    case cmdToggleHooksTrashLog:
                        syncConfig.IsHooksTrashLogEnabled = !syncConfig.IsHooksTrashLogEnabled;
                        Console.WriteLine($"{nameof(syncConfig.IsHooksTrashLogEnabled)} was set to {syncConfig.IsHooksTrashLogEnabled}");
                        break;
                    case cmdRestartServer:
                        result = serverManager.TryRestartServer(out textResult);
                        if (result == false) Console.WriteLine($"Unable to restart the server. Reason: {textResult}");
                        else Console.WriteLine("The server is restarting");
                        break;
                    case cmdStopServer:
                        result = serverManager.TryStopServer(out textResult);
                        if (result == false) Console.WriteLine($"Unable to stop the server. Reason: {textResult}");
                        else Console.WriteLine("The server is stopping");
                        break;
                }

                Console.WriteLine();
                PrintThatLogModeIsRunning();
                
                WinAPIWrapper.QuickEditMode(false);
            }

        }
    }

    private void PrintThatLogModeIsRunning()
    {
        if (syncConfig.IsServerLogEnabled)
        {
            Console.WriteLine("=== LOG MODE ===" + Environment.NewLine + 
                              "Press 'Ctrl + C' to enter 'Command Mode'");
        }
    }

    public string? ReadLine()
    {
        return Console.ReadLine();
    }

    public void WriteErrorLine(string text)
    {
        Console.Error.WriteLine(text);
    }
    
    public void WriteLine(string text = "")
    {
        Console.WriteLine(text);
    }
    
    public void WriteLineSynchronizedWithoutWaiting(string text = "")
    {
        if (Monitor.TryEnter(syncConfig.ConsoleLock))
        {
            try
            {
                Console.WriteLine(text);
            }
            finally
            {
                Monitor.Exit(syncConfig.ConsoleLock);
            }
        }
    }
    
    public void WriteTextSynchronizedWithoutWaiting(string text = "")
    {
        if (Monitor.TryEnter(syncConfig.ConsoleLock))
        {
            try
            {
                Console.Write(text);
            }
            finally
            {
                Monitor.Exit(syncConfig.ConsoleLock);
            }
        }
    }
}