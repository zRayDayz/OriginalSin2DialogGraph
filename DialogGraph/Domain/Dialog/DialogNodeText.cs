using System.Diagnostics;

namespace DialogGraph;

[DebuggerDisplay("({AdditionalText}) {TextId}: {Text}")]
public struct DialogNodeText
{
    public DialogNodeText(string text)
    {
        TextId = null;
        Text = text;
        AdditionalText = null;
    }
    
    public DialogNodeText(string textId, string text, string? additionalText = null)
    {
        TextId = textId;
        Text = text;
        AdditionalText = additionalText;
    }
    
    public string? TextId;
    public string Text;
    public string? AdditionalText;
}