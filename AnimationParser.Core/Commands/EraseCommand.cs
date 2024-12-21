namespace AnimationParser.Core.Commands;

/// <summary>
/// Represents a command that erases an object from the animation context.
/// The object is removed from the context and will not be drawn after
/// this command is executed.
/// </summary>
public class EraseCommand : ISemanticallySingleCommand
{
    public string ObjectName { get; }

    public EraseCommand(string objectName)
    {
        ObjectName = objectName;
    }

    public void Execute(AnimationContext context)
    {
        context.EraseObject(ObjectName);
    }
}
