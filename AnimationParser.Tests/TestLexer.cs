using AnimationParser.Core;
using AnimationParser.Core.Tokens;

namespace AnimationParser.Tests;

public class TestLexer
{
    private MockedTokenFactory tokenFactory = null!;

    [SetUp]
    public void SetUp()
    {
        tokenFactory = new MockedTokenFactory();
    }

    [TearDown]
    public void TearDown()
    {
        tokenFactory = null!;
    }

    private void VerifyTokenStream(IEnumerable<Token> lhs, IEnumerable<Token> rhs)
    {
        var lhsEnumerator = lhs.GetEnumerator();
        var rhsEnumerator = rhs.GetEnumerator();

        while (lhsEnumerator.MoveNext() && rhsEnumerator.MoveNext())
        {
            if (!lhsEnumerator.Current.Equals(rhsEnumerator.Current))
            {
                throw new AssertionException($"Expected {rhsEnumerator.Current}, but got {lhsEnumerator.Current}");
            }
        }

        if (lhsEnumerator.MoveNext() || rhsEnumerator.MoveNext())
        {
            throw new AssertionException("Token streams have different lengths");
        }
    }

    [Test]
    public void TestLexer_SingleLineSupport()
    {
        const string input = @"(define cross ( (line (0 0) (50 50)) ) )";
        Token[] expected = [
            tokenFactory.LeftParen,
                tokenFactory.Keyword("define"),
                tokenFactory.Identifier("cross"),
                tokenFactory.LeftParen,
                    tokenFactory.LeftParen,
                        tokenFactory.Keyword("line"),
                        tokenFactory.LeftParen,
                            tokenFactory.Number("0"),
                            tokenFactory.Number("0"),
                        tokenFactory.RightParen,
                        tokenFactory.LeftParen,
                            tokenFactory.Number("50"),
                            tokenFactory.Number("50"),
                            tokenFactory.RightParen,
                        tokenFactory.RightParen,
                    tokenFactory.RightParen,
                tokenFactory.RightParen,
            tokenFactory.EndOfSource,
        ];

        var lexer = new Lexer(input);

        var tokens = lexer.Tokenize();

        VerifyTokenStream(tokens, expected);
    }

    [Test]
    public void TestLexer_PanicIfContainingInvalidCharacter()
    {
        const string input = @"(define cross ( (line (0 0) (50 50)) ) )*/";

        Lexer lexer = new(input);

        Assert.That(() =>
        {
            // Since the lexer is lazy evaluated, we need to enumerate the token stream to trigger the exception
            _ = lexer.Tokenize().All(_ => true);
        }, Throws.Exception);
    }

    [Test]
    public void TestLexer_MultiLineSupport()
    {
        const string input = @"(define cross 
            ( (line (0 0) (50 50)) ) )";

        Token[] expected = [
            tokenFactory.LeftParen,
                tokenFactory.Keyword("define"),
                tokenFactory.Identifier("cross"),
                tokenFactory.LeftParen,
                    tokenFactory.LeftParen,
                        tokenFactory.Keyword("line"),
                        tokenFactory.LeftParen,
                            tokenFactory.Number("0"),
                            tokenFactory.Number("0"),
                        tokenFactory.RightParen,
                        tokenFactory.LeftParen,
                            tokenFactory.Number("50"),
                            tokenFactory.Number("50"),
                        tokenFactory.RightParen,
                    tokenFactory.RightParen,
                tokenFactory.RightParen,
            tokenFactory.RightParen,
            tokenFactory.EndOfSource,
        ];

        var lexer = new Lexer(input);

        var tokens = lexer.Tokenize();

        VerifyTokenStream(tokens, expected);
    }

    [Test]
    public void TestLexer_NumberWithDecimalDot()
    {
        const string input = @"(42.0 3.14)";
        Token[] expected = [
            tokenFactory.LeftParen,
                tokenFactory.Number("42.0"),
                tokenFactory.Number("3.14"),
            tokenFactory.RightParen,
            tokenFactory.EndOfSource,
        ];

        var lexer = new Lexer(input);
        var tokens = lexer.Tokenize();

        VerifyTokenStream(tokens, expected);
    }

    [Test]
    public void TestLexer_NumberWithNegativeSign()
    {
        const string input = @"(-42 -3.14)";
        Token[] expected = [
            tokenFactory.LeftParen,
                tokenFactory.Number("-42"),
                tokenFactory.Number("-3.14"),
            tokenFactory.RightParen,
            tokenFactory.EndOfSource,
        ];

        var lexer = new Lexer(input);
        var tokens = lexer.Tokenize();

        VerifyTokenStream(tokens, expected);
    }

    [Test]
    public void TestLexer_IdentifierWithNumber()
    {
        const string input = "object1";
        Token[] expected = [
            tokenFactory.Identifier("object1"),
            tokenFactory.EndOfSource,
        ];

        var lexer = new Lexer(input);
        var tokens = lexer.Tokenize();

        VerifyTokenStream(tokens, expected);
    }

    [Test]
    public void TestTokenPosition_NoNewLineOrSpace()
    {
        const string input = "object";

        var lexer = new Lexer(input);
        var token = lexer.Tokenize().Single();

        Assert.That(token.Position, Is.EqualTo(new TokenPosition
        {
            LineNo = 1,
            CharNo = 1,
        }));
    }

    [Test]
    public void TestTokenPosition_WithSpaces()
    {
        const string input = "   object  "; // 3 leading and 2 trailing

        var lexer = new Lexer(input);
        var token = lexer.Tokenize().Single();

        Assert.That(token.Position, Is.EqualTo(new TokenPosition
        {
            LineNo = 1,
            CharNo = 4,
        }));
    }

    [Test]
    public void TestTokenPosition_WithNewLines()
    {
        const string input = """
                             
                                object1
                             
                             """;

        var lexer = new Lexer(input);
        var token = lexer.Tokenize().Single();

        Assert.That(token.Position, Is.EqualTo(new TokenPosition
        {
            LineNo = 2,
            CharNo = 4,
        }));
    }
}

file static class TokenSequenceExtensions
{
    public static IEnumerable<Token> RemoveEndOfSource(this IEnumerable<Token> tokens)
        => tokens.Where(static t => t.Type is not TokenType.EndOfSource);

    public static Token Single(this IEnumerable<Token> tokens)
        => Enumerable.Single(tokens.RemoveEndOfSource());
}
