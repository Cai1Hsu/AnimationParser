namespace AnimationParser.Core.Tokens;

/// <summary>
/// Represents the type of a token.
/// </summary>
public enum TokenType
{
    LeftParen,
    RightParen,
    Keyword,
    Identifier,
    Number,
    EndOfSource
}
