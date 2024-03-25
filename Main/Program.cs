using System.Diagnostics;
using System.Windows;
using DialogGraph;

namespace Main
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            Console.WriteLine("Press 'Enter' to start");
            Console.ReadLine();
            
            try
            {
                Injector.TryInjectLogic();
                
                WinAPIWrapper.ToggleConsoleVisibility(false);
                
                Application wpfApp = new App();
                wpfApp.Run();
            }
            catch (Exception e)
            {
                WinAPIWrapper.ToggleConsoleVisibility(true);
                Console.WriteLine(e);
                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Press 'ENTER' to exit");
                Console.ReadLine();
            }

        }
        
    }
}
