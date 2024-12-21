using System.Runtime.CompilerServices;

namespace AnimationParser.Core.Tokens;

/// <summary>
/// Represents a factory that creates tokens. Handles the
/// position and source index of the tokens.
/// </summary>
public class TokenFactory
{
    public TokenPosition CurrentPosition { get; protected set; } = new TokenPosition
    {
        LineNo = 1,
        CharNo = 1,
    };

    public int CurrentSourceIndex { get; protected set; }

    public string SourceDocument { get; protected set; }

    public TokenFactory(string sourceDocument)
    {
        CurrentSourceIndex = 0;
        SourceDocument = sourceDocument;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void BeginToken()
        => startPosition = CurrentPosition;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EndToken()
        => startPosition = null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private TokenPosition TakeStartPosition()
    {
        var position = startPosition!.Value;
        EndToken();

        return position;
    }

    private TokenPosition? startPosition;

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void MoveNext()
    {
        CurrentPosition = new TokenPosition
        {
            LineNo = CurrentPosition.LineNo,
            CharNo = CurrentPosition.CharNo + 1,
        };
        CurrentSourceIndex++;

        // if (startPosition.LineNo == 1 && startPosition.CharNo == 1)
        // {
        //     startPosition = CurrentPosition;
        // }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void OnNewLine()
    {
        CurrentPosition = new TokenPosition
        {
            LineNo = CurrentPosition.LineNo + 1,
            CharNo = 0,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    protected int GetTokenStartIndex(int length)
    {
        if (CurrentSourceIndex == 0)
        {
            return 0;
        }

        return CurrentSourceIndex - length;
    }

    public virtual Token LeftParen
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        get
        {
            Token token = new(SourceDocument)
            {
                Type = TokenType.LeftParen,
                Position = TakeStartPosition(),
                SourceIndex = GetTokenStartIndex(1),
                TextLength = 1,
            };

            return token;
        }
    }

    public virtual Token RightParen
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        get
        {
            Token token = new(SourceDocument)
            {
                Type = TokenType.RightParen,
                Position = TakeStartPosition(),
                SourceIndex = GetTokenStartIndex(1),
                TextLength = 1
            };

            return token;
        }
    }

    public virtual Token EndOfSource
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        get
        {
            Token token = new(SourceDocument)
            {
                Type = TokenType.EndOfSource,
                Position = TakeStartPosition(),
                SourceIndex = GetTokenStartIndex(0),
                TextLength = 0
            };

            return token;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public virtual Token Keyword(int length)
    {
        Token token = new(SourceDocument)
        {
            Type = TokenType.Keyword,
            Position = TakeStartPosition(),
            SourceIndex = GetTokenStartIndex(length),
            TextLength = length
        };

        return token;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public virtual Token Identifier(int length)
    {
        Token token = new(SourceDocument)
        {
            Type = TokenType.Identifier,
            Position = TakeStartPosition(),
            SourceIndex = GetTokenStartIndex(length),
            TextLength = length
        };

        return token;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public virtual Token Number(int length)
    {
        Token token = new(SourceDocument)
        {
            Type = TokenType.Number,
            Position = TakeStartPosition(),
            SourceIndex = GetTokenStartIndex(length),
            TextLength = length
        };

        return token;
    }
}