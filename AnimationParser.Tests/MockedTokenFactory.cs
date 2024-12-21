using AnimationParser.Core.Tokens;

namespace AnimationParser.Tests;

public class MockedTokenFactory : TokenFactory
{
    public MockedTokenFactory()
        : base(null!)
    {
    }

    private Token MoveForward(Token token)
    {
        for (int i = 0; i < token.TextLength; i++)
            MoveNext();

        return token;
    }

    public override Token LeftParen => MoveForward(new Token("(")
    {
        Type = TokenType.LeftParen,
        Position = CurrentPosition,
        SourceIndex = 0,
        TextLength = 1,
    });

    public override Token RightParen => MoveForward(new Token(")")
    {
        Type = TokenType.RightParen,
        Position = CurrentPosition,
        SourceIndex = 0,
        TextLength = 1,
    });

    public override Token EndOfSource => MoveForward(new Token
    {
        Type = TokenType.EndOfSource,
        Position = CurrentPosition,
        SourceIndex = 0,
        TextLength = 0,
    });

    public Token Identifier(string text) => MoveForward(new Token(text)
    {
        Type = TokenType.Identifier,
        Position = CurrentPosition,
        SourceIndex = 0,
        TextLength = text.Length,
    });

    public Token Keyword(string text) => MoveForward(new Token(text)
    {
        Type = TokenType.Keyword,
        Position = CurrentPosition,
        SourceIndex = 0,
        TextLength = text.Length,
    });

    public Token Number(string text) => MoveForward(new Token(text)
    {
        Type = TokenType.Number,
        Position = CurrentPosition,
        SourceIndex = 0,
        TextLength = text.Length,
    });
}
