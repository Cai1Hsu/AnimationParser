using System.Runtime.CompilerServices;
using AnimationParser.Core.Tokens;

namespace AnimationParser.Core;

/// <summary>
/// The lexer is responsible for converting the input text into a sequence of tokens.
/// Mainly, it is responsible for skipping whitespace and comments, and identifying
/// keywords, identifiers, numbers, and symbols.
/// This implementation is optimized for performance and minimal memory allocations.
/// And is stream-based, with stackless coroutines, meaning it doesn't create a list of tokens in memory.
/// Also, all token uses the same underlying string, so no matter how long the input is, how many tokens are created,
/// the memory usage is constant, which is the size of the stackless coroutine.
/// </summary>
public class Lexer
{
    private readonly string sourceDocument;

    private int CurrentSourceIndex => tokenFactory.CurrentSourceIndex;

    private char CurrentChar => CurrentSourceIndex < sourceDocument.Length ? sourceDocument[CurrentSourceIndex] : '\0';

    private readonly TokenFactory tokenFactory;

    public Lexer(string input)
    {
        sourceDocument = input;
        tokenFactory = new TokenFactory(input);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void MoveNext()
    {
        tokenFactory.MoveNext();
    }

    /// <summary>
    /// Tokenize the input text. Using a stackless coroutine to generate tokens.
    /// This method is lazy, meaning it will only generate tokens when you iterate over the result.
    /// </summary>
    /// <returns>A token each time the MoveNext method is called.</returns>
    /// <exception cref="Exception">If an invalid character is found.</exception>
    public IEnumerable<Token> Tokenize()
    {
        while (CurrentSourceIndex < sourceDocument.Length)
        {
            switch (CurrentChar)
            {
                case '\n':
                    tokenFactory.OnNewLine();
                    goto case ' ';
                case ' ':
                case '\t':
                case '\r':
                    MoveNext();
                    continue;

                case '(':
                    tokenFactory.BeginToken();
                    MoveNext();
                    yield return tokenFactory.LeftParen;
                    continue;

                case ')':
                    tokenFactory.BeginToken();
                    MoveNext();
                    yield return tokenFactory.RightParen;
                    continue;

                default:
                    tokenFactory.BeginToken();
                    if (char.IsLetter(CurrentChar))
                    {
                        yield return ReadKeywordOrIdentifier();
                    }
                    else if (char.IsDigit(CurrentChar))
                    {
                        yield return ReadNumber();
                    }
                    else if (CurrentChar == '-')
                    {
                        yield return ReadTokenStartWithMinus();
                    }
                    else
                    {
                        throw new Exception($"Invalid character '{CurrentChar}' at line {tokenFactory.CurrentPosition.LineNo}:{tokenFactory.CurrentPosition.CharNo}");
                    }
                    continue;
            }
        }

        tokenFactory.BeginToken();

        yield return tokenFactory.EndOfSource;
    }

    /// <summary>
    /// Read a token that starts with a minus sign.
    /// Currently only support negative numbers.
    /// </summary>
    /// <returns>A token representing a token starting with a minus sign.</returns>
    /// <exception cref="Exception">If the token can not be parsed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private Token ReadTokenStartWithMinus()
    {
        MoveNext();

        if (char.IsDigit(CurrentChar))
        {
            var numberToken = ReadNumber();

            numberToken.SourceIndex--; // Move back to include the minus sign
            numberToken.TextLength++; // Include the minus sign in the length

            return numberToken;
        }
        else
        {
            throw new Exception($"Invalid character '{CurrentChar}' at line {tokenFactory.CurrentPosition.LineNo}:{tokenFactory.CurrentPosition.CharNo}");
        }
    }

    /// <summary>
    /// Read a keyword or an identifier. Identifiers can contain letters, digits, and underscores.
    /// </summary>
    /// <returns>A token representing a keyword or an identifier.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private Token ReadKeywordOrIdentifier()
    {
        int start = CurrentSourceIndex;
        while (char.IsLetterOrDigit(CurrentChar) || CurrentChar == '_')
            MoveNext();

        int length = CurrentSourceIndex - start;

        return IsKeyword(sourceDocument.AsSpan(start, length)) switch
        {
            true => tokenFactory.Keyword(length),
            false => tokenFactory.Identifier(length),
        };
    }

    /// <summary>
    /// Read a number. A number can contain digits and a dot.
    /// </summary>
    /// <returns>A token representing a number.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private Token ReadNumber()
    {
        int start = CurrentSourceIndex;
        while (char.IsDigit(CurrentChar) || CurrentChar == '.')
            MoveNext();

        return tokenFactory.Number(CurrentSourceIndex - start);
    }

    /// <summary>
    /// Check if the given text is a keyword.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <returns>True if the text is a keyword, otherwise false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private bool IsKeyword(ReadOnlySpan<char> text)
    {
        // Static dispatch to avoid string allocations
        switch (text)
        {
            case "define":
            case "place":
            case "shift":
            case "erase":
            case "loop":
            case "line":
            case "circle":
            case "left":
            case "right":
            case "up":
            case "down":
                return true;
            default:
                return false;
        }
    }
}
