using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace DialogGraph;

// MarkupExtension allows to conveniently pass additional arguments (to a constructor or regular properties) into this class if needed
// Theoretically, the alternative would be to use an ObjectDataProvider, but this is less convenient
public class GraphLayoutCreatorExtension : MarkupExtension
{
    public string Name { get; set; }
    public Brush Background { get; set; }
    public Binding GraphBinding { get; set; }
    public Binding LayoutAlgorithmTypeBinding { get; set; }

    public Binding LayoutParametersBinding { get; set; }
    
    public string OverlapRemovalAlgorithmType { get; set; }
    
    public Binding HighlightAlgorithmFactoryBinding { get; set; }
    
    public string HighlightAlgorithmType { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var graphLayoutFactory = IOCContainer.Instance.GraphLayoutFactory.Value;
        graphLayoutFactory.Name = Name;
        graphLayoutFactory.Background = Background;
        graphLayoutFactory.GraphBinding = GraphBinding;
        graphLayoutFactory.LayoutAlgorithmTypeBinding = LayoutAlgorithmTypeBinding;
        graphLayoutFactory.LayoutParametersBinding = LayoutParametersBinding;
        graphLayoutFactory.OverlapRemovalAlgorithmType = OverlapRemovalAlgorithmType;
        graphLayoutFactory.HighlightAlgorithmFactoryBinding = HighlightAlgorithmFactoryBinding;
        graphLayoutFactory.HighlightAlgorithmType = HighlightAlgorithmType;
        return graphLayoutFactory.CreateGraphLayout(serviceProvider);
    }
}