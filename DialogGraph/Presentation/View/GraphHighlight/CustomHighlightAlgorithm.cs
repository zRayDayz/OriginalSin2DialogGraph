using GraphShape.Algorithms.Highlight;
using QuikGraph;

namespace DialogGraph;

// The class is based on the original SimpleHighlightAlgorithm class
public class CustomHighlightAlgorithm<TVertex, TEdge, TGraph> : HighlightAlgorithmBase<TVertex, TEdge, TGraph, IHighlightParameters>
    where TEdge : IEdge<TVertex>
    where TGraph : class, IBidirectionalGraph<TVertex, TEdge>
    where TVertex : IGraphVertex 
{
    public CustomHighlightAlgorithm(
        IHighlightController<TVertex, TEdge, TGraph> controller,
        IHighlightParameters parameters)
        : base(controller, parameters)
    {
    }

    private void ClearSemiHighlights()
    {
        // Origin
        //foreach (TVertex vertex in Controller.SemiHighlightedVertices.ToArray())
        foreach (TVertex vertex in Controller.SemiHighlightedVertices)
        {
            Controller.RemoveSemiHighlightFromVertex(vertex);
        }

        // Origin
        //foreach (TEdge edge in Controller.SemiHighlightedEdges.ToArray())
        foreach (TEdge edge in Controller.SemiHighlightedEdges)
        {
            Controller.RemoveSemiHighlightFromEdge(edge);
        }
    }

    private void ClearAllHighlights()
    {
        ClearSemiHighlights();

        // Origin
        //foreach (TVertex vertex in Controller.HighlightedVertices.ToArray())
        foreach (TVertex vertex in Controller.HighlightedVertices)
        {
            Controller.RemoveHighlightFromVertex(vertex);
        }

        // Origin
        //foreach (TEdge edge in Controller.HighlightedEdges.ToArray())
        foreach (TEdge edge in Controller.HighlightedEdges)
        {
            Controller.RemoveHighlightFromEdge(edge);
        }
    }

    private TVertex ClearAllHighlightsAndFindConstantlyHighlightedVertex()
    {
        ClearSemiHighlights();

        TVertex activeVertex = default;
        foreach (TVertex vertex in Controller.HighlightedVertices)
        {
            Controller.RemoveHighlightFromVertex(vertex);
            if (vertex.IsConstantlyHighlighted) activeVertex = vertex;
        }

        foreach (TEdge edge in Controller.HighlightedEdges)
        {
            Controller.RemoveHighlightFromEdge(edge);
        }

        return activeVertex;
    }

    public override void ResetHighlight()
    {
        ClearAllHighlights();
    }
    
    public override bool OnVertexHighlighting(TVertex vertex)
    {
        // Origin
        //ClearAllHighlights();

        if (!Controller.Graph.ContainsVertex(vertex))
            return false;
        
        foreach (TEdge edge in Controller.Graph.InEdges(vertex))
        {
            Controller.SemiHighlightEdge(edge, "InEdge");
            if (EqualityComparer<TVertex>.Default.Equals(edge.Source, vertex)
                || Controller.IsHighlightedVertex(edge.Source))
            {
                continue;
            }

            Controller.SemiHighlightVertex(edge.Source, "Source");
        }
        
        foreach (TEdge edge in Controller.Graph.OutEdges(vertex))
        {
            Controller.SemiHighlightEdge(edge, "OutEdge");
            if (EqualityComparer<TVertex>.Default.Equals(edge.Target, vertex)
                || Controller.IsHighlightedVertex(edge.Target))
            {
                continue;
            }

            Controller.SemiHighlightVertex(edge.Target, "Target");
        }

        Controller.HighlightVertex(vertex, "None");
        return true;
    }

    public override bool OnVertexHighlightRemoving(TVertex vertex)
    {
        if (!Controller.Graph.ContainsVertex(vertex))
            return false;

        // Origin
        //ClearAllHighlights();
        
        var activeVertex = ClearAllHighlightsAndFindConstantlyHighlightedVertex();
        if(activeVertex != null) OnVertexHighlighting(activeVertex);

        return true;
    }
    
    public override bool OnEdgeHighlighting(TEdge edge)
    {
        // Origin
        //ClearAllHighlights();

        if (!Controller.Graph.ContainsEdge(edge))
            return false;

        if (edge.Source.IsConstantlyHighlighted || edge.Target.IsConstantlyHighlighted) return false;
        

        Controller.HighlightEdge(edge, null);
        Controller.SemiHighlightVertex(edge.Source, "Source");
        Controller.SemiHighlightVertex(edge.Target, "Target");
        return true;
    }
    
    public override bool OnEdgeHighlightRemoving(TEdge edge)
    {
        if (!Controller.Graph.ContainsEdge(edge))
            return false;
        
        if (edge.Source.IsConstantlyHighlighted || edge.Target.IsConstantlyHighlighted) return false;
        
        // Origin
        //ClearAllHighlights();
        
        var activeVertex = ClearAllHighlightsAndFindConstantlyHighlightedVertex();
        if(activeVertex != null) OnVertexHighlighting(activeVertex);

        return true;
    }
}