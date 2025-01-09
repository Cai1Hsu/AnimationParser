#nullable enable

using osu.Framework.Graphics;
using NUnit.Framework;
using AnimationParser.Core;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.UserInterface;
using System;
using osuTK.Graphics;
using osu.Framework.Input.Events;
using System.Linq;

namespace AnimationParser.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneAnimationScene : AnimationParserTestScene
    {
        private Box background = null!;
        private AnimationScene scene = null!;
        private CodeTextBox codeTextBox = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex("#535C72"),
                },
            };
        }
        private partial class CodeTextBox : BasicTextBox
        {
            public CodeTextBox()
            {
                Text = """
                       (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
                       (place cross (50 50))
                       (loop 5 ( (shift cross right) (shift cross up) (shift cross left) (shift cross down)) )
                       (erase cross)
                       """;
                PlaceholderText = "Enter code here";
                Size = new(800, 40);
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (previousCode is not null)
                {
                    Text = previousCode;
                    previousCode = null;
                }

                return base.OnClick(e);
            }

            private string? previousCode = null;
            public void SetErrorMessage(string message)
            {
                if (previousCode is null)
                {
                    previousCode = Text;
                }

                Text = message;
                this.FlashColour(Color4.Red, 800, Easing.OutQuint);
            }
        }

        protected override void LoadComplete()
        {
            Add(scene = new AnimationScene()
            {
                RelativePositionAxes = Axes.Both,
                RelativeSizeAxes = Axes.None,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new(500, 500),
            });
            Add(codeTextBox = new CodeTextBox());
        }

        protected override void Update()
        {
            base.Update();

            var ex = scene.AnimationExecutionException;
            scene.ClearException();
            if (ex is not null) {
                codeTextBox.SetErrorMessage(ex.Message);
            }
        }

        [Test]
        public void TestInterpretCode()
        {
            AddStep("clear", () =>scene.Clear());

            AddStep("begin animation", () =>
            {
                try
                {
                    var lexer = new Lexer(codeTextBox.Text);
                    var parser = new Parser(lexer.Tokenize());
                    scene.ExecuteAnimationCommands(parser.Parse());

                    background.FlashColour(Color4.White, 800, Easing.OutQuint);
                }
                catch (Exception e)
                {
                    codeTextBox.SetErrorMessage(e.Message);
                }
            });

            AddUntilStep("wait for complete", () => scene.Children.All(c => !c.Transforms.Any()));
        }
    }
}
