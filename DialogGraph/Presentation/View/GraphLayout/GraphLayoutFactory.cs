using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Xaml;
using GraphShape.Controls;

namespace DialogGraph;

// This class helps to abstract the View from a specific GraphLayout implementation
// This is important because, for example, CustomGraphLayout uses specific Generic parameters from the ViewModel
public class GraphLayoutFactory : IGraphLayoutFactory
{
    public string Name { get; set; }
    public Brush Background { get; set; }
    public Binding GraphBinding { get; set; }
    public Binding LayoutAlgorithmTypeBinding { get; set; }
    public Binding LayoutParametersBinding { get; set; }
    public string OverlapRemovalAlgorithmType { get; set; }
    public Binding HighlightAlgorithmFactoryBinding { get; set; }
    public string HighlightAlgorithmType { get; set; }

    public GraphCanvas CreateGraphLayout(IServiceProvider serviceProvider)
    {
        var graphLayout = new CustomGraphLayout();

        if (Name != null)
        {
            graphLayout.Name = Name;
            var rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            ((Window)rootObjectProvider.RootObject).RegisterName(Name, graphLayout);
        }
        
        if (Background != null) graphLayout.Background = Background;
        
        if (GraphBinding != null) graphLayout.SetBinding(CustomGraphLayout.GraphProperty, GraphBinding);
        
        if (LayoutAlgorithmTypeBinding != null) graphLayout.SetBinding(CustomGraphLayout.LayoutAlgorithmTypeProperty, LayoutAlgorithmTypeBinding);
        
        if (LayoutParametersBinding != null) graphLayout.SetBinding(CustomGraphLayout.LayoutParametersProperty, LayoutParametersBinding);
        
        if (OverlapRemovalAlgorithmType != null) graphLayout.OverlapRemovalAlgorithmType = OverlapRemovalAlgorithmType;
        
        if (HighlightAlgorithmFactoryBinding != null) graphLayout.SetBinding(CustomGraphLayout.HighlightAlgorithmFactoryProperty, HighlightAlgorithmFactoryBinding);
        
        if (HighlightAlgorithmType != null) graphLayout.HighlightAlgorithmType = HighlightAlgorithmType;
        
        return graphLayout;
    }
}