using System.Diagnostics;

namespace DialogGraph;

[DebuggerDisplay("{Id}")]
public class DialogNode
{
    public DialogNode(string type, string id, DialogNodeText[] dialogNodeTexts, string[] tempChildrenIds, string fullData)
    {
        Type = type;
        Id = id;
        DialogNodeTexts = dialogNodeTexts;
        TempChildrenIds = tempChildrenIds;
        FullData = fullData;
    }
    
    public DialogNode(string type, string id, string endNode, string speakerIndex, DialogNodeText[] dialogNodeTexts, DialogFlag[] flagsToCheck, DialogFlag[] flagsToSet, string[] tempChildrenIds, string fullData)
    {
        Type = type;
        Id = id;
        EndNode = endNode == "1";
        SpeakerIndex = speakerIndex;
        DialogNodeTexts = dialogNodeTexts;
        FlagsToCheck = flagsToCheck;
        FlagsToSet = flagsToSet;
        TempChildrenIds = tempChildrenIds;
        FullData = fullData;
    }
    
    public string Type;
    public DialogNodeType StrictType;
    public string Id;
    public bool EndNode = false;
    public string SpeakerIndex = String.Empty;
    public DialogNodeText[] DialogNodeTexts;
    public DialogFlag[] FlagsToCheck = Array.Empty<DialogFlag>();
    public DialogFlag[] FlagsToSet = Array.Empty<DialogFlag>();
    public string[] TempChildrenIds;
    public string FullData;
    
    public DialogNode[] Children = Array.Empty<DialogNode>();
    public string SpeakerName = String.Empty;
    public string SpeakerId = String.Empty;

}