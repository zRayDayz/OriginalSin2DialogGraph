using GraphShape.Algorithms.Highlight;

namespace DialogGraph;

// The class is based on the original StandardHighlightAlgorithmFactory class
// For additional comments as to why this class is not Generic read the comments on the CustomGraph class
public class CustomHighlightAlgorithmFactory : IHighlightAlgorithmFactory<GraphVertex, GraphEdge, CustomGraph>
{
    protected const string SimpleMode = "Simple";
    
    public virtual IEnumerable<string> HighlightModes { get; } = new[] { SimpleMode };
    
    public virtual IHighlightAlgorithm<GraphVertex, GraphEdge> CreateAlgorithm(
        string highlightMode,
        IHighlightContext<GraphVertex, GraphEdge, CustomGraph> context,
        IHighlightController<GraphVertex, GraphEdge, CustomGraph> controller,
        IHighlightParameters parameters)
    {
        if (highlightMode is null)
            throw new ArgumentNullException(nameof(highlightMode));
        if (context is null)
            throw new ArgumentNullException(nameof(context));
        if (controller is null)
            throw new ArgumentNullException(nameof(controller));

        switch (highlightMode)
        {
            case SimpleMode:
                return new CustomHighlightAlgorithm<GraphVertex, GraphEdge, CustomGraph>(controller, parameters);
        }

        return null;
    }
    
    public virtual IHighlightParameters CreateParameters(string highlightMode, IHighlightParameters parameters)
    {
        if (highlightMode is null)
            throw new ArgumentNullException(nameof(highlightMode));

        switch (highlightMode)
        {
            case "":
            case SimpleMode:
                return new HighlightParameters();
        }

        return null;
    }
    
    public virtual bool IsValidMode(string mode)
    {
        return mode == string.Empty || HighlightModes.Contains(mode);
    }
    
    public virtual string GetHighlightMode(IHighlightAlgorithm<GraphVertex, GraphEdge> algorithm)
    {
        if (algorithm is null)
            throw new ArgumentNullException(nameof(algorithm));

        if (algorithm is CustomHighlightAlgorithm<GraphVertex, GraphEdge, CustomGraph>)
            return SimpleMode;
        
        return null;
    }
}