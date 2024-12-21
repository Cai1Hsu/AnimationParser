using System.Runtime.CompilerServices;
using AnimationParser.Core;
using AnimationParser.Core.Commands;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class BenchmarkExecute
{
    private readonly IEnumerator<ISemanticallySingleCommand> commands;
    private readonly AnimationContext context = new AnimationContext();

    public BenchmarkExecute()
    {
        var tokens = new Lexer(Program.SampleCode).Tokenize().ToList();
        var parser = new Parser(tokens);

        commands = parser.Parse().ToList().Flatten().GetEnumerator();
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoOptimization)]
    public void BenchmarkExecuting()
    {
        // parser is lazy evaluated, so we need to consume the command sequence
        while (commands.MoveNext())
        {
            commands.Current.Execute(context);
        }
    }
}
