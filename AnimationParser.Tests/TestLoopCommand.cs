using AnimationParser.Core;

namespace AnimationParser.Tests;

public class TestLoopCommand
{
    [Test]
    public void TestLoopCommand_ShouldExecuteMultipleTimes()
    {
        const string code = """
                            (define drawable ((line (0 0) (50 50))))
                            (loop 42 ( (shift drawable up) ) )
                            """;

        var context = new ShiftMockedContext();

        var lexer = new Lexer(code);

        lexer.Tokenize().InterprettedlyExecuteAll(context);

        Assert.That(context.LoopCount, Is.EqualTo(42));
    }

    private class ShiftMockedContext : AnimationContext
    {
        public int LoopCount { get; private set; }

        protected override void OnObjectShifting(string name, AnimationObject drawable, Direction direction)
        {
            if (name == "drawable")
            {
                LoopCount++;
            }
            else
            {
                throw new Exception($"Unexpected drawable name: {name}");
            }
        }
    }
}
