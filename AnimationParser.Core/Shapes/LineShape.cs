using System.Numerics;

namespace AnimationParser.Core.Shapes;

/// <summary>
/// Represents a line shape in the animation.
/// Game engines may use this class to create their internal bindings.
/// </summary>
public class LineShape : IShape
{
    public ShapeType Type => ShapeType.Line;

    public Vector2 Start { get; }

    public Vector2 End { get; }

    public LineShape(Vector2 start, Vector2 end)
    {
        Start = start;
        End = end;
    }
}
