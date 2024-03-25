using System.Diagnostics;

namespace DialogGraph;

[DebuggerDisplay("{SpeakerName}: {Id}")]
public struct DialogSpeaker
{
    public DialogSpeaker(string id, string index)
    {
        Id = id;
        Index = index;
    }
    
    public string SpeakerName { get; set; } = String.Empty;
    public string Id { get; }
    public string Index { get; }
}