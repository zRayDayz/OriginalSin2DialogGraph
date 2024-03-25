namespace DialogGraph;

public struct GraphMessage : IMediatorMessage
{
    public CustomGraph graph;
    public bool isNewGraph;

    public GraphMessage(CustomGraph graph, bool isNewGraph)
    {
        this.graph = graph;
        this.isNewGraph = isNewGraph;
    }
}