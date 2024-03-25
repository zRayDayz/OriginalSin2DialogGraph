using System.Diagnostics;
using System.Windows.Media;
using GraphShape.Utils;

namespace DialogGraph
{
    [DebuggerDisplay("{Id}")]
    public class GraphVertex : NotifierObject, IGraphVertex
    {
        public GraphVertex(string id, string text, string speakerId, DialogNodeType type, bool isEndNode, GraphDialogFlag[] graphDialogFlagsToCheck, GraphDialogFlag[] graphDialogFlagsToSet, string fullInfo, string speakerName = "")
        {
            Id = id;
            Text = text;
            SpeakerId = speakerId;
            this.speakerName = speakerName;
            Type = type;
            IsEndNode = isEndNode;
            GraphDialogFlagsToCheck = graphDialogFlagsToCheck;
            GraphDialogFlagsToSet = graphDialogFlagsToSet;

            NodeInfo = "Id: " + id + Environment.NewLine +
                       "Text: " + text + Environment.NewLine +
                       "==========" + Environment.NewLine +
                       fullInfo;
        }

        public virtual void InitializeViewProperties(Color backgroundColor, Color foregroundColor)
        {
            this.backgroundColor = backgroundColor;
            this.foregroundColor = foregroundColor;
        }

        public virtual string Id { get; }
        public virtual string Text { get; }
        public virtual string SpeakerId { get; }

        protected string speakerName;
        public virtual string SpeakerName
        {
            get => speakerName;
            set => ViewSpeakerName = value;
        }
        public virtual string ViewSpeakerName
        {
            get
            {
                return speakerName == String.Empty ? SpeakerId : speakerName;
            }
            set
            {
                speakerName = value;
                OnPropertyChanged();
            }
        }

        protected bool isActiveNode = false;
        public virtual bool IsActiveNode
        {
            get => isActiveNode;
            set
            {
                isActiveNode = value;
                OnPropertyChanged();
            } 
        }
        
        public bool IsConstantlyHighlighted => IsActiveNode;

        public DialogNodeType Type { get; }
        public bool IsEndNode { get; }

        private Color backgroundColor;
        public virtual SolidColorBrush BackgroundColor
        {
            get => new SolidColorBrush(backgroundColor);
        }
        
        private Color foregroundColor;
        public virtual SolidColorBrush ForegroundColor
        {
            get => new SolidColorBrush(foregroundColor);
        }

        public GraphDialogFlag[] GraphDialogFlagsToCheck { get; }
        public GraphDialogFlag[] GraphDialogFlagsToSet { get; }
        
        public virtual string NodeInfo { get; }

        public override string ToString()
        {
            return Id;
        }
    }
}