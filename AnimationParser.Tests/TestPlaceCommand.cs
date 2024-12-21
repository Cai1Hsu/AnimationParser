using System.Numerics;
using AnimationParser.Core;

namespace AnimationParser.Tests;

public class TestPlaceCommand
{
    [Test]
    public void TestPlaceCommand_ShouldMoveToNewPosition()
    {
        const string code = """
                            (define drawable ((line (0 0) (50 50))))
                            (place drawable (10 10))
                            """;

        var context = new PlaceMockedContext();

        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());

        var commands = parser.Parse().Select(c => c as ISemanticallySingleCommand).ToList();

        commands[0]!.Execute(context);
        Assert.That(context.GetObject("drawable")!.Position, Is.EqualTo(Vector2.Zero));

        commands[1]!.Execute(context);
        Assert.That(context.GetObject("drawable")!.Position, Is.EqualTo(new Vector2(10, 10)));
    }

    private class PlaceMockedContext : AnimationContext
    {
        public Vector2? newPosition;

        protected override void OnObjectPlaced(string name, AnimationObject value, Vector2 position)
        {
            base.OnObjectPlaced(name, value, position);

            if (name == "drawable")
            {
                newPosition = position;
            }
            else
            {
                throw new Exception($"Unexpected drawable name: {name}");
            }
        }
    }

    [Test]
    public void TestPlaceCommand_ThrowIfObjectNotExist()
    {
        const string code = "(place drawable (10 10))";

        var lexer = new Lexer(code);

        Assert.Throws<Exception>(() => lexer.Tokenize().InterprettedlyExecuteAll());
    }
}
