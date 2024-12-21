namespace AnimationParser.Core.Commands;

/// <summary>
/// Represents a command that defines an object.
/// </summary>
public class DefineCommand : ISemanticallySingleCommand
{
    public string ObjectName { get; }

    public AnimationObject Object { get; set; }

    public DefineCommand(string objectName, AnimationObject obj)
    {
        ObjectName = objectName;
        Object = obj;
    }

    public void Execute(AnimationContext context)
    {
        context.DeclareObject(ObjectName, Object);
    }
}
