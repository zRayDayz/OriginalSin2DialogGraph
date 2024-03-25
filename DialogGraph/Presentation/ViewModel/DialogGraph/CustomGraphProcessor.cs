using System.Linq.Expressions;
using System.Windows.Media;

namespace DialogGraph;

public class CustomGraphProcessor
{
    private MainWindowViewModel viewModel;
    private Dictionary<string, Color> flagColorsByIds = new Dictionary<string, Color>(16);
    private Color[] colorPalette;
    private int colorCounter;

    public CustomGraphProcessor(IMediator mediator, MainWindowViewModel viewModel)
    {
        this.viewModel = viewModel;
        mediator.Register<GraphMessage>(PrepareAndSetGraphToView);
        colorPalette = new[]
        {
            Colors.MediumPurple, 
            Colors.Crimson, 
            Colors.Khaki,
            Colors.Olive,
            Colors.DimGray,
            Colors.Indigo,
            Colors.Chocolate,
            Colors.Orchid,
            Colors.Cyan,
            Colors.Navy, 
        };
        colorCounter = colorPalette.Length;
    }

    private void PrepareAndSetGraphToView(GraphMessage arg)
    {
        var graph = arg.graph;
        if (arg.isNewGraph)
        {
            flagColorsByIds.Clear();
            colorCounter = colorPalette.Length;
            foreach (GraphVertex vertex in graph.Vertices)
            {
                (Color backgroundColor, Color foregroundColor) = ChooseVertexColorByType(vertex.Type, vertex.IsEndNode);
                vertex.InitializeViewProperties(backgroundColor, foregroundColor);
                SetDialogFlagColor(vertex, flagColorsByIds);
            }
        }
        viewModel.Graph = graph;
    } 
    
    /* Colors in Divinity Original Sin 2 SDK Dialog Viewer
     * Greeting         - green - 46 139 87
     * Question         - blue - 176 196 222
     * Answer           - orange - 244 164 96
     * Jump             - gray blue - 133 141 152
     * End node         - red - 205 92 92
     * Persuasion       - light orange - 255 218 185
     * PersuasionResult - very light orange - 255 239 213
     * Pop              - purple - 128 0  128
     * DualDialog       - pink - 255 105 180
     */
    private (Color backgroundColor, Color foregroundColor) ChooseVertexColorByType(DialogNodeType type, bool isEndNode)
    {
        if (isEndNode) return (Colors.Brown, Colors.White);
        switch (type)
        {
            case DialogNodeType.Greeting:
                return (Colors.GreenYellow, Colors.Black);
            case DialogNodeType.Question:
                return (Colors.RoyalBlue, Colors.White);
            case DialogNodeType.Answer:
                return (Colors.Orange, Colors.Black);
            case DialogNodeType.Jump:
                return (Colors.LightSteelBlue, Colors.Black);
            case DialogNodeType.Persuasion:
                return (Colors.DarkSalmon, Colors.Black);
            case DialogNodeType.PersuasionResult:
                return (Colors.Bisque, Colors.Black);
            case DialogNodeType.Pop:
                return (Colors.Purple, Colors.White);
            case DialogNodeType.DualDialog:
                return (Colors.Pink, Colors.Black);
            case DialogNodeType.UNRECOGNIZED:
                return (Colors.White, Colors.Black);
            default:
                throw new NotSupportedException("A new enum entry has been added, now it is necessary to add a color for it as well");
        }
    }

    private void SetDialogFlagColor(GraphVertex vertex, in Dictionary<string, Color> flagColorsByIds)
    {
        var flagsToCheck = vertex.GraphDialogFlagsToCheck;
        for (int i = 0; i < flagsToCheck.Length; i++)
        {
            GraphDialogFlag flag = flagsToCheck[i];
            if (flagColorsByIds.TryGetValue(flag.Id, out Color value))
            {
                flag.InitializedViewProperty(value);
                continue;
            }
            var color = GetColorFromPalette();
            flag.InitializedViewProperty(color);
            flagColorsByIds.Add(flag.Id, color);
        }
        
        var flagsToSet = vertex.GraphDialogFlagsToSet;
        for (int i = 0; i < flagsToSet.Length; i++)
        {
            GraphDialogFlag flag = flagsToSet[i];
            if (flagColorsByIds.TryGetValue(flag.Id, out Color value))
            {
                flag.InitializedViewProperty(value);
                continue;
            }
            var color = GetColorFromPalette();
            flag.InitializedViewProperty(color);
            flagColorsByIds.Add(flag.Id, color);
        }
    }
    
    private Color GetColorFromPalette()
    {
        if (colorCounter == 0)
        {
            return Colors.Magenta;
        }

        var color = colorPalette[colorCounter - 1];
        colorCounter--;
        return color;
    }
}