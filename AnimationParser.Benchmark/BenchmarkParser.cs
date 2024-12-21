using System.Runtime.CompilerServices;
using AnimationParser.Core;
using AnimationParser.Core.Commands;
using AnimationParser.Core.Tokens;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class BenchmarkParser
{
    private readonly List<Token> tokens;

    public BenchmarkParser()
    {
        tokens = new Lexer(Program.SampleCode).Tokenize().ToList();
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoOptimization)]
    public IAnimationCommand BenchmarkParsing()
    {
        // parser is lazy evaluated, so we have to consume all tokens
        return new Parser(tokens).Parse().Last();
    }
}
