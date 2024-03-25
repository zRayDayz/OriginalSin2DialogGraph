using System.ComponentModel;
using System.Runtime.CompilerServices;
using QuikGraph;

namespace DialogGraph
{
    // It is possible to make this class Generic but then there will be problems with IHighlightAlgorithmFactory<TVertex, TEdge, TGraph> and its limitations in Generic parameters
    // If we make this class Generic, then an instance of this class still cannot be cast to the IMutableBidirectionalGraph<IGraphVertex, IEdge<IGraphVertex>> type, because TEdge is not covariant in it, so it is hard to work with this class using abstraction
    // Therefore it was decided to make this class non-Generic, but to make all its members virtual so that it could play the role of a base class and was extensible by its child classes
    public class CustomGraph : BidirectionalGraph<GraphVertex, GraphEdge>, INotifyPropertyChanged, ICustomGraph
    {
        public CustomGraph(string id, string name)
        {
            Id = id;
            Name = name;

            graphInfoHeader = "Dialog graph info:" + Environment.NewLine +
                              "Id: " + id + Environment.NewLine +
                              "Name: " + name + Environment.NewLine;
            dialogInfo = graphInfoHeader;
        }

        private string graphInfoHeader;
        
        public virtual string Name { get; }
        public virtual string Id { get; }
        
        protected string dialogInfo;
        public virtual string DialogInfo
        {
            get => dialogInfo;
            set
            {
                dialogInfo = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual GraphVertex ActiveNode { get; protected set; }

        public virtual void SetNewActiveNode(GraphVertex vertex)
        {
            if (ActiveNode != null) ActiveNode.IsActiveNode = false;
            vertex.IsActiveNode = true;
            ActiveNode = vertex;

        }

        public virtual void UpdateDialogInfo(GraphVertex vertex)
        {
            string info = graphInfoHeader + "==========" + Environment.NewLine +
                          "Node full info: " + Environment.NewLine +
                          vertex.NodeInfo;
            DialogInfo = info;
        }
        
        
        
        
        
        
        
        
        

        
        /// <summary>
        /// Not in use yet
        /// </summary>
        public virtual bool AddIVertex(IGraphVertex vertex)
        {
            if(vertex is not GraphVertex) throw new ArgumentException($"{nameof(vertex)} argument must be of type {nameof(GraphVertex)}. To work with class data, it is better to use a factory");
            return AddVertex((GraphVertex)vertex);
        }
        
        /// <summary>
        /// Not in use yet
        /// </summary>
        public virtual bool AddIEdge(IGraphEdge edge)
        {
            if(edge is not GraphEdge) throw new ArgumentException($"{nameof(edge)} argument must be of type {nameof(GraphEdge)}. To work with class data, it is better to use a factory");
            return AddEdge((GraphEdge)edge);
        }


    }
}