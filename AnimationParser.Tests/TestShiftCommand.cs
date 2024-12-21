using AnimationParser.Core;

namespace AnimationParser.Tests;

public class TestShiftCommand
{
    [Test]
    public void TestShiftCommand_ShouldMoveDrawable()
    {
        const string code = """
                            (define drawable ((line (0 0) (50 50))))
                            (shift drawable up)
                            """;

        var context = new ShiftMockedContext();

        var lexer = new Lexer(code);

        lexer.Tokenize().InterprettedlyExecuteAll(context);

        Assert.That(context.ShiftDirection, Is.EqualTo(Direction.Up));
    }

    private class ShiftMockedContext : AnimationContext
    {
        public Direction? ShiftDirection { get; private set; }

        protected override void OnObjectShifting(string name, AnimationObject drawable, Direction direction)
        {
            if (name == "drawable")
            {
                ShiftDirection = direction;
            }
            else
            {
                throw new Exception($"Unexpected drawable name: {name}");
            }
        }
    }

    [Test]
    public void TestShiftCommand_ThrowIfObjectNotExist()
    {
        const string code = "(shift drawable up)";

        var lexer = new Lexer(code);

        Assert.Throws<Exception>(() => lexer.Tokenize().InterprettedlyExecuteAll());
    }
}
