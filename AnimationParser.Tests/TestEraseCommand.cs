using AnimationParser.Core;
using AnimationParser.Core.Commands;

namespace AnimationParser.Tests;

public class TestEraseCommand
{
    [Test]
    public void TestEraseCommand_ShouldRemoveDrawable()
    {
        const string code = """
                            (define drawable ((line (0 0) (50 50))))
                            (erase drawable)
                            """;

        var context = new EraseMockedContext();

        var lexer = new Lexer(code);
        var parser = new Parser(lexer.Tokenize());

        var commands = parser.Parse().Select(c => c as ISemanticallySingleCommand).ToList();

        commands[0]!.Execute(context);
        Assert.That(context.GetObject("drawable"), Is.Not.Null);

        commands[1]!.Execute(context);

        Assert.That(context.GetObject("drawable"), Is.Null);
        Assert.That(context.IsErasedExecuted, Is.True);
    }

    private class EraseMockedContext : AnimationContext
    {
        public bool IsErasedExecuted { get; private set; }

        protected override void OnObjectErasingOut(string name, AnimationObject drawable)
        {
            base.OnObjectErasingOut(name, drawable);

            if (name == "drawable")
            {
                IsErasedExecuted = true;
            }
            else
            {
                throw new Exception($"Unexpected drawable name: {name}");
            }
        }
    }

    [Test]
    public void TestEraseCommand_ThrowIfObjectNotExist()
    {
        const string code = "(erase drawable)";

        var lexer = new Lexer(code);

        Assert.Throws<Exception>(() => lexer.Tokenize().InterprettedlyExecuteAll());
    }
}
