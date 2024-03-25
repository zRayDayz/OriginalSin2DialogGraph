namespace DialogGraph;

public class LocalizationProcessor
{
    private ILocalizationFileParser localizationFileParser;
    private Dictionary<string, string> textAndIds = new Dictionary<string, string>(128);

    public bool? ShoudLocalizeText => localizationFileParser.CanLocalizationBeUsed;

    public LocalizationProcessor(ILocalizationFileParser localizationFileParser)
    {
        this.localizationFileParser = localizationFileParser;
    }
    
    public string? TryGetLocalizationStringById(string id)
    {
        if (textAndIds.TryGetValue(id, out string foundText)) return foundText;
        
        var text = localizationFileParser.TryCreateLocalizationStringById(id);
        if (text == null) return null;
        textAndIds.Add(id, text);
        return text;
    }
}