using System.Diagnostics;

namespace DialogGraph;

[DebuggerDisplay("{Name}")]
public struct Dialog
{
    public Dialog(string name, string id, List<DialogNode> dialogNodes)
    {
        this.Name = name;
        this.Id = id;
        this.DialogNodes = dialogNodes;
    }

    public string Name { get; }
    public string Id { get; }
    public List<DialogNode> DialogNodes { get; }
}