using System.Windows.Data;
using System.Windows.Media;
using GraphShape.Controls;

namespace DialogGraph;

public interface IGraphLayoutFactory
{
    public string Name { get; set; }
    public Brush Background { get; set; }
    public Binding GraphBinding { get; set; }
    public Binding LayoutAlgorithmTypeBinding { get; set; }
    public Binding LayoutParametersBinding { get; set; }
    public string OverlapRemovalAlgorithmType { get; set; }
    public Binding HighlightAlgorithmFactoryBinding { get; set; }
    public string HighlightAlgorithmType { get; set; }
    
    public GraphCanvas CreateGraphLayout(IServiceProvider serviceProvider);
}