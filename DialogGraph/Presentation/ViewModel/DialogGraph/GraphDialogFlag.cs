using System.Windows.Media;

namespace DialogGraph;

public class GraphDialogFlag
{
    public GraphDialogFlag(string id, string fullData)
    {
        Id = id;
        FullData = "Flag full info: " + Environment.NewLine + fullData;
    }

    public virtual void InitializedViewProperty(Color backgroundColor)
    {
        this.backgroundColor = backgroundColor;
    }
    
    public virtual string Id { get; }
    
    public virtual string FullData { get; }

    private Color backgroundColor;
    public virtual SolidColorBrush BackgroundColor
    {
        get => new SolidColorBrush(backgroundColor);
    }
}