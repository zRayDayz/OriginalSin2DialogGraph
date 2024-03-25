namespace DialogGraph;

public interface ILocalizationFileParser
{
    public bool? CanLocalizationBeUsed { get; }
    public string? TryCreateLocalizationStringById(string id);
}