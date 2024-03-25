using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using DialogGraph;

namespace Sandbox;

public class TestWPFApp
{
    public void StartWpfApp()
    {
        Application wpfApp = new App();
        TestAppInitializer.PrepareTestInitializer();
        
        //DrawTestGraph(wpfApp);
        DrawSpecificGraphFromDialogFile(wpfApp);
        
        wpfApp.Run();
    }

    private void DrawSpecificGraphFromDialogFile(Application wpfApp)
    {
        var dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal, wpfApp.Dispatcher);
        dispatcherTimer.Tick += delegate(object sender, EventArgs args)
        {
            //var testArg = Encoding.UTF8.GetBytes("{ \"dialogName\": \"EG_LucianDallisBraccus\" }");
            var testArg = Encoding.UTF8.GetBytes("{ \"dialogName\": \"TUT_CargoDeck_MagisterGuard\" }");
            var dataReceiver = IOCContainer.Instance.NetworkClientManager.Value.DataReceiver;
            dataReceiver.ParseReceivedData(null, testArg);
            (sender as DispatcherTimer).Stop();
        };
        dispatcherTimer.Interval = TimeSpan.FromSeconds(5);
        dispatcherTimer.Start();
    }
        
    private void DrawTestGraph(Application wpfApp)
    {
        var dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal, wpfApp.Dispatcher);
        dispatcherTimer.Tick += TestGraphCreator;
        dispatcherTimer.Interval = TimeSpan.FromSeconds(5);
        dispatcherTimer.Start();
    }
    
    private void TestGraphCreator(object sender, EventArgs args)
    {
        var graph = new CustomGraph("dialogId", "dialogName");
    
        GraphVertex[] vertices = Enumerable.Range(0, 8).Select(VertexFactory).ToArray();
        graph.AddVertexRange(vertices);
        graph.AddEdgeRange(new []
        {
            EdgeFactory(vertices[0], vertices[1]),
            EdgeFactory(vertices[1], vertices[2]),
            EdgeFactory(vertices[2], vertices[3]),
            EdgeFactory(vertices[2], vertices[4]),
            EdgeFactory(vertices[0], vertices[5]),
            EdgeFactory(vertices[1], vertices[7]),
            EdgeFactory(vertices[4], vertices[6]),
            EdgeFactory(vertices[0], vertices[4])
        });
    
        //Graph = graph;
        IOCContainer.Instance.MainWindowViewModel.Value.Graph = graph;
        vertices[4].IsActiveNode = true;
        
        (sender as DispatcherTimer).Stop();
    }
    
    static GraphVertex VertexFactory(int vertex)
    {
        List<GraphDialogFlag> flagsToCheck = new List<GraphDialogFlag>(2);
        var graphDialogFlag = new GraphDialogFlag("abcdef-123456-ghijkl-7890!@", "Full data of flag 1");
        graphDialogFlag.InitializedViewProperty(Colors.Gold);
        flagsToCheck.Add(graphDialogFlag);
        
        graphDialogFlag = new GraphDialogFlag("abcdef-123456-ghijkl-7890!@", "Full data of flag 2");
        graphDialogFlag.InitializedViewProperty(Colors.Firebrick);
        flagsToCheck.Add(graphDialogFlag);
        
        List<GraphDialogFlag> flagsToSet = new List<GraphDialogFlag>(2);
        graphDialogFlag = new GraphDialogFlag("abcdef-123456-ghijkl-7890!@", "Full data of flag 3");
        graphDialogFlag.InitializedViewProperty(Colors.Chartreuse);
        flagsToSet.Add(graphDialogFlag);
        
        graphDialogFlag = new GraphDialogFlag("abcdef-123456-ghijkl-7890!@", "Full data of flag 4");
        graphDialogFlag.InitializedViewProperty(Colors.CornflowerBlue);
        flagsToSet.Add(graphDialogFlag);
        
        //return new PocVertex(vertex.ToString());
        var graphVertex = new GraphVertex(vertex.ToString(), $"My first sentence.{Environment.NewLine}My second sentence.{Environment.NewLine}My third sentence.{Environment.NewLine}4th{Environment.NewLine}5th", "123", DialogNodeType.Answer, false, 
            flagsToCheck.ToArray(), flagsToSet.ToArray(), "full info of this node");
        graphVertex.InitializeViewProperties(Colors.Bisque, Colors.Black);
        return graphVertex;
    }
    
    static GraphEdge EdgeFactory(GraphVertex source, GraphVertex target)
    {
        return new GraphEdge($"{source.Id}to{target.Id}", source, target);
    }
}