using System.Diagnostics;

namespace DialogGraph;

[DebuggerDisplay("{Id}")]
public struct DialogFlag
{
    public DialogFlag(string id, string fullData)
    {
        Id = id;
        FullData = fullData;
    }
    
    public string Id { get; }
    public string FullData { get; }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public bool Equals(DialogFlag other)
    {
        return Id == other.Id;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not DialogFlag other) return false;
        return Equals(other);
    }
    
}