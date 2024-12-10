using System.Numerics;
using AnimationParser.Core.Shapes;

namespace AnimationParser.Core;

/// <summary>
/// Represents an object in the animation. Game engines
/// may use this object to create their internal bindings.
/// </summary>
public class AnimationObject
{
    public List<IShape> Shapes { get; } = [];

    public Vector2 Position { get; set; }

    public void AddShape(IShape shape)
    {
        Shapes.Add(shape);
    }
}
