namespace DialogGraph;

public readonly record struct NameAndGuid(string name, string guid);

public struct NetworkDialogGraphData
{
    public string DialogName { get; set; }
    public string CurrentDialogOptionName { get; set; }
    public string[] namesAndGuidsCombined { get; set; }
    public NameAndGuid[] NamesAndGuids { get; set; }
}

