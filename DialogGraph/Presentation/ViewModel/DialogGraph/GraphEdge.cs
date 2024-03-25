using QuikGraph;
using System.Diagnostics;

namespace DialogGraph
{
    [DebuggerDisplay("{" + nameof(Source) + "." + nameof(GraphVertex.Id) + "} -> {" + nameof(Target) + "." + nameof(GraphVertex.Id) + "}")]
    public class GraphEdge : Edge<GraphVertex>, IGraphEdge
    {
        public virtual string Id { get; }
        
        public GraphEdge(string id, GraphVertex source, GraphVertex target)
            : base(source, target)
        {
            Id = id;
        }
    }
}