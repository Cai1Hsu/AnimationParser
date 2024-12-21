using AnimationParser.Core;

namespace AnimationParser.Tests;

public class TestDefineCommand
{
    [Test]
    public void TestDefineCommand_WhenDrawableIsDeclared_ShouldAddDrawableToContext()
    {
        const string code = "(define drawable ((line (0 0) (50 50))))";

        var context = new DefineMockedContext();

        var lexer = new Lexer(code);

        lexer.Tokenize().InterprettedlyExecuteAll(context);

        Assert.That(context.IsDrawableAdded, Is.True);
        Assert.That(context.GetObject("drawable"), Is.Not.Null);
    }


    private class DefineMockedContext : AnimationContext
    {
        public bool IsDrawableAdded { get; private set; }

        protected override void OnObjectAdded(string name, AnimationObject value)
        {
            base.OnObjectAdded(name, value);

            if (name == "drawable")
            {
                IsDrawableAdded = true;
            }
            else
            {
                throw new Exception($"Unexpected drawable name: {name}");
            }
        }
    }

    [Test]
    public void TestDefineCommand_ThrowIfDefineDuplicate()
    {
        const string code = """
                            (define drawable ((line (0 0) (50 50))))
                            (define drawable ((line (0 0) (50 50))))
                            """;

        var lexer = new Lexer(code);

        Assert.Throws<Exception>(() => lexer.Tokenize().InterprettedlyExecuteAll());
    }

    [Test]
    public void TestDefineCommand_NotThrowIfEraseBeforeDuplicate()
    {
        const string code = """
                            (define drawable ((line (0 0) (50 50))))
                            (erase drawable)
                            (define drawable ((line (0 0) (50 50))))
                            """;

        var lexer = new Lexer(code);

        Assert.DoesNotThrow(() => lexer.Tokenize().InterprettedlyExecuteAll());
    }
}
