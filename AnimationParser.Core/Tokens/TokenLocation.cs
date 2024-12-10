namespace AnimationParser.Core.Tokens;

/// <summary>
/// Represents the position of a token in the source document.
/// </summary>
public struct TokenPosition
{
    public int LineNo { get; internal set; }

    public int CharNo { get; internal set; }
}
