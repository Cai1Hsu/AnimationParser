using System.Numerics;

namespace AnimationParser.Core.Commands;

/// <summary>
/// Represents a command that places an object at a specific position.
/// </summary>
public class PlaceCommand : ISemanticallySingleCommand
{
    public string ObjectName { get; }
    public Vector2 Position { get; }

    public PlaceCommand(string objectName, Vector2 position)
    {
        ObjectName = objectName;
        Position = position;
    }

    public void Execute(AnimationContext context)
    {
        context.PlaceObject(ObjectName, Position);
    }
}
