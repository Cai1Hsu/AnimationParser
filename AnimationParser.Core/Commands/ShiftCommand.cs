namespace AnimationParser.Core.Commands;

/// <summary>
/// Represents a command that shifts an object in a direction.
/// </summary>
public class ShiftCommand : IAnimationCommand
{
    public string ObjectName { get; }

    public Direction Direction { get; }

    public ShiftCommand(string objectName, Direction direction)
    {
        ObjectName = objectName;
        Direction = direction;
    }

    public void Execute(AnimationContext context)
    {
        context.ShiftObject(ObjectName, Direction);
    }
}
