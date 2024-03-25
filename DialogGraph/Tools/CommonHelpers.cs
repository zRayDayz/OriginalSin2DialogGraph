using System.Text;

namespace DialogGraph;

public static class CommonHelpers
{
    private static StringBuilder joinSpanStringBuilder = new StringBuilder();
    public static string JoinSpan(ReadOnlySpan<byte> span, string separator, int maxLength = -1)
    {
        if (joinSpanStringBuilder.Length > 4096)
        {
            joinSpanStringBuilder = new StringBuilder();
        }
        else joinSpanStringBuilder.Clear();

        int length;
        if (maxLength < 0) length = span.Length;
        else if (maxLength > span.Length) length = span.Length;
        else length = maxLength;
        
        int i;
        for (i = 0; i < length - 1; i++)
        {
            joinSpanStringBuilder.Append(span[i]);
            joinSpanStringBuilder.Append(separator);
        }
        joinSpanStringBuilder.Append(span[i]);
        return joinSpanStringBuilder.ToString();
    }
}