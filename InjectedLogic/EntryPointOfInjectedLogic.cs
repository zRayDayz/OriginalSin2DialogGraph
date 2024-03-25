
namespace InjectedLogic
{
    class EntryPointOfInjectedLogic
    {
        // This method will be called by CoreCLR after the injection
        public static int Bootstrap(IntPtr argument, int size)
        {
            var entryPointClass = new EntryPointOfInjectedLogic();
            entryPointClass.Main();
            
            return 0;
        }

        public void Main()
        {
            WinAPIWrapper.AllocConsole();

            try
            {
                if (Environment.Is64BitProcess == false) throw new Exception("The assembly code for the injected logic is not ready to work with the x32 process. Only x64 is supported.");
                
                Console.WriteLine("Injection initialization");
                Console.WriteLine("Press 'Enter' to start");
                Console.ReadLine();

                IOCContainer.CreateIOCContainer();

                IOCContainer.Instance.ConfigManager.Value.InitializeConfig();
                IOCContainer.Instance.HooksSetter.Value.SetHooks();

                StartServerAndConsole();

            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }

            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Press 'ENTER' to exit");
            Console.ReadLine();
        }

        private void StartServerAndConsole()
        {
            IOCContainer.Instance.ServerManager.Value.TryStartServer(out string _);

            IOCContainer.Instance.ConsoleProcessor.Value.RunConsole();
            
            IOCContainer.Instance.ServerManager.Value.TryStopServer(out string _);
        }
    }
}

