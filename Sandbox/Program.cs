using System.Buffers;
using System.Windows;
using DialogGraph;

namespace Sandbox;

public class Program
{
    [STAThread]
    public static void Main()
    {
        Console.WriteLine("Press 'Enter' to start");
        Console.ReadLine();

        try
        {
            new TestWPFApp().StartWpfApp();
            
            //var serverAndConsoleTest = new ServerClientConsoleTest();

            //serverAndConsoleTest.StartServerClientAndConsole();
            //serverAndConsoleTest.StartServerAndConsole();

            //var dialogProcessingTest = new DialogProcessingTest();
            //dialogProcessingTest.TestLocalizationReading();
            //dialogProcessingTest.TestDialogProcessing();

            //new ServerConsoleWPFTest().Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Console.WriteLine(Environment.NewLine);
        }

        Console.WriteLine("Press 'ENTER' to exit");
        Console.ReadLine();
    }


}