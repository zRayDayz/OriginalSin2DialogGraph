using System.Diagnostics;
using System.Runtime.InteropServices;

namespace InjectedLogic;

/* It is Test version of fixed string. Unfortunately, most of the lines in Divinity Original Sin 2 are over 60 characters long, so the solution was not to use the FixedString structure
 * With C# 12 "inline arrays" it is possible to create a fixed-size buffers in a struct without an unsafe context: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct#inline-arrays
 * About fixed-size buffers: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/unsafe-code#fixed-size-buffers

 * The real size of the structure is 32 bytes, i.e. 15 * 2 = 30 bytes for the char array, and 2 bytes for the length due to data alignment (can be checked in sharplab.io using Inspect.Stack(), it looks like Marshal.SizeOf() is lying)
 * Theoretically, to save on memory (since the structure should not be large), is is possible to use a compression algorithm like Huffman Coding
 * Either all strings (their characters) can be kept in one large array and using instances of Memory class refer to certain parts of this array thus "creating" strings

 * Instead of using the "fixed" operator, it is possible to use MemoryMarshal.CreateSpan(). It is almost equally fast and safe
 */
[DebuggerDisplay("{GetStringUTF8}")]
public unsafe struct TEST_FixedString
{
    public TEST_FixedString(Span<char> spanStr, byte length)
    {
        if (length <= 0 || length > StrMaxLength) throw new ArgumentOutOfRangeException(nameof(length), $"The length of a newline cannot exceed the maximum allowed length or be less than or equal to 0. Given length: {length}. Maximum length: {StrMaxLength}");
        if (spanStr.Length < length) throw new ArgumentOutOfRangeException(nameof(length), $"The length of characters taken from Span cannot exceed the length of Span. Length of Span: {spanStr.Length}. Given length: {length}");

        fixed (char* strPointer = str)
        {
            var strSpanWrapper = new Span<char>(strPointer, StrMaxLength);
            var payloadOfArgStrSpan = spanStr.Slice(0, length);
            payloadOfArgStrSpan.CopyTo(strSpanWrapper);
            this.Length = length;
        }
        
    }
    
    public TEST_FixedString(Span<char> spanStr)
    {
        if (spanStr.Length <= 0 || spanStr.Length > StrMaxLength) throw new ArgumentOutOfRangeException(nameof(spanStr.Length), $"The length of the new string (from Span) cannot exceed the maximum allowed length. Given length: {spanStr.Length}. Maximum length: {StrMaxLength}");

        fixed (char* strPointer = str)
        {
            var strSpanWrapper = new Span<char>(strPointer, StrMaxLength);
            spanStr.CopyTo(strSpanWrapper);
            this.Length = (byte)spanStr.Length;
        }
    }
    
    public const byte StrMaxLength = 15;
    private fixed char str[StrMaxLength];
    public byte Length { get; }

    public Span<char> GetSpanString
    {
        get
        {
            fixed (char* strPointer = str)
            {
                return new Span<char>(strPointer, Length);
            }
        }
    }
    
    string GetStringUTF8
    {
        get
        {
            if (Length == 0) return String.Empty;
            ReadOnlySpan<char> strSpanWrapper = MemoryMarshal.CreateReadOnlySpan(ref str[0], Length);
            return new string(strSpanWrapper);
        }
    }

    public override string ToString()
    {
        return GetStringUTF8;
    }
}