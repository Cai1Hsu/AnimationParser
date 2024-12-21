using System.Runtime.CompilerServices;
using AnimationParser.Core;
using AnimationParser.Core.Commands;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class BenchmarkExecute
{
    private readonly List<IAnimationCommand> commands;
    private readonly IEnumerator<IAnimationCommand> commandEnumerator;
    private readonly AnimationContext context = new AnimationContext();

    public BenchmarkExecute()
    {
        var tokens = new Lexer(Program.CodeSample).Tokenize().ToList();
        var parser = new Parser(tokens);

        commands = parser.Parse().ToList();
        commandEnumerator = commands.GetEnumerator();
    }

    [Benchmark]
    [MethodImpl(MethodImplOptions.NoOptimization)]
    public void BenchmarkParsing()
    {
        while (commandEnumerator.MoveNext())
        {
            commandEnumerator.Current.Execute(context);
        }
    }
}
