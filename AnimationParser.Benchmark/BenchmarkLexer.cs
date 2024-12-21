using System.Runtime.CompilerServices;
using AnimationParser.Core;
using AnimationParser.Core.Tokens;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class BenchmarkLexer
{
    [Benchmark]
    [MethodImpl(MethodImplOptions.NoOptimization)]
    public Token BenchmarkLexing()
    {
        // lexer is lazy evaluated, so we need to consume the token stream
        return new Lexer(Program.SampleCode).Tokenize().Last();
    }
}
