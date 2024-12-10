using System.Diagnostics;
using System.Globalization;

namespace AnimationParser.Core.Tokens;

/// <summary>
/// Represents a token in the source document.
/// </summary>
public struct Token
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string sourceDocument;

    public TokenType Type { get; internal set; }
    public readonly ReadOnlySpan<char> Text => sourceDocument.AsSpan(SourceIndex, Length);

    public TokenPosition Position { get; internal set; }

    public int SourceIndex { get; internal set; }

    public int Length { get; internal set; }

    public Token(string sourceDocument)
    {
        this.sourceDocument = sourceDocument;
    }

    public override readonly string ToString() => Type switch
    {
        TokenType.Keyword or
        TokenType.Identifier or
        TokenType.Number => $"{Type}(\"{Text}\") at line {Position.LineNo}:{Position.CharNo}",
        _ => $"{Type} at line {Position.LineNo}:{Position.CharNo}",
    };

    public override readonly int GetHashCode()
        => HashCode.Combine(Type, CultureInfo.InvariantCulture.CompareInfo.GetHashCode(Text, CompareOptions.None));

    public override bool Equals(object? obj)
    {
        if (obj is null || obj is not Token token)
        {
            return false;
        }

        if (Type != token.Type)
            return false;

        return CompareText(token);
    }

    private bool CompareText(Token them)
    {
        if (Text.IsEmpty != them.Text.IsEmpty)
            return false;

        if (Text.Length != them.Text.Length)
            return false;

        for (int i = 0; i < Text.Length; i++)
        {
            if (Text[i] != them.Text[i])
                return false;
        }

        return true;
    }
}
