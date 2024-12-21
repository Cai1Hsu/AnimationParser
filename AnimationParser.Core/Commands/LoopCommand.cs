namespace AnimationParser.Core.Commands;

/// <summary>
/// Represents a command that loops a set of commands a specified number of times.
/// Zero or negative counts will still be parsed, but is ignored by the flatten method.
/// </summary>
public class LoopCommand : IAnimationCommand
{
    public int Count { get; }
    public IEnumerable<IAnimationCommand> Commands { get; }

    public LoopCommand(int count, IEnumerable<IAnimationCommand> commands)
    {
        Count = count;
        Commands = commands;
    }
}
