using System.Runtime.CompilerServices;
using AnimationParser.Core;
using AnimationParser.Core.Commands;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class BenchmarkParser
{
    private readonly IEnumerator<IAnimationCommand> commandSequence;

    public BenchmarkParser()
    {
        var tokens = new Lexer(Program.CodeSample).Tokenize().ToList();
        var parser = new Parser(tokens);

        commandSequence = parser.Parse().GetEnumerator();
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoOptimization)]
    public void BenchmarkParsing()
    {
        // parser is lazy evaluated, so we have to consume all tokens
        while (commandSequence.MoveNext())
        {
            _ = commandSequence.Current;
        }
    }
}
