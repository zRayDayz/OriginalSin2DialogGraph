using System.Windows;
using System.Windows.Input;
using GraphShape.Controls;

namespace DialogGraph;

public class CustomGraphLayout : GraphLayout<GraphVertex, GraphEdge, CustomGraph>
{
    public CustomGraphLayout()
    {
        HighlightAttachedProperty.AddActiveNodeHighlightTriggeredHandler(this, OnHighlightTriggered);
    }

    #region Active node highlight
    // Basically the same approach as in GraphLayout<TVertex,TEdge,TGraph>.OnHighlightTriggered()
    private void OnHighlightTriggered(object sender, RoutedEventArgs eventArgs)
    {
        if (eventArgs is not HighlightTriggeredEventArgs args) throw new ArgumentException($"The routed argument was expected to be of type {nameof(HighlightTriggeredEventArgs)}");
        if (Graph is null || HighlightAlgorithm is null) return;

        if (args.OriginalSource is VertexControl vertexControl)
        {
            OnVertexHighlightTriggered(vertexControl, args);
        }
    }

    private void OnVertexHighlightTriggered(VertexControl vertexControl, HighlightTriggeredEventArgs args)
    {
        var vertex = vertexControl.Vertex as GraphVertex;
        if (vertex == null || !Graph.ContainsVertex(vertex))
            return;

        if (args.IsPositiveTrigger)
        {
            HighlightAlgorithm.OnVertexHighlighting(vertex);
        }
        else
        {
            HighlightAlgorithm.OnVertexHighlightRemoving(vertex);
        }
    }
    #endregion

    
    // Alternatively, it would be possible to use Behaviour from the Style element
    // For example using: https://stackoverflow.com/questions/1647815/how-to-add-a-blend-behavior-in-a-style-setter or https://stackoverflow.com/questions/942548/setting-a-property-with-an-eventtrigger
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && e.RightButton == MouseButtonState.Released && e.Source is VertexControl vertex)
        {
            Graph.UpdateDialogInfo((GraphVertex)vertex.Vertex);
        }
        
        base.OnPreviewMouseLeftButtonDown(e);
    }
}