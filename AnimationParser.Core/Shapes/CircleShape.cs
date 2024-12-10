using System.Numerics;

namespace AnimationParser.Core.Shapes;

/// <summary>
/// Represents a shape in the animation.
/// Game engines may use this interface to create their internal bindings.
/// </summary>
public class CircleShape : IShape
{
    public ShapeType Type => ShapeType.Circle;

    public Vector2 Center { get; }

    public float Radius { get; }

    public CircleShape(Vector2 center, float radius)
    {
        Center = center;
        Radius = radius;
    }
}
