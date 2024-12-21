using System.Runtime.CompilerServices;
using AnimationParser.Core;
using AnimationParser.Core.Tokens;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class BenchmarkLexer
{
    private readonly Lexer lexer = new Lexer(Program.CodeSample);
    private readonly IEnumerator<Token> tokens;

    public BenchmarkLexer()
    {
        tokens = lexer.Tokenize().GetEnumerator();
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoOptimization)]
    public void BenchmarkLexing()
    {
        // lexer is lazy evaluated, so we need to consume the token stream
        while (tokens.MoveNext())
        {
            _ = tokens.Current;
        }
    }
}
