#nullable enable

using System.Collections.Generic;
using osuTK;
using AnimationParser.Core;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using System;
using System.Linq;
using osuTK.Graphics;
using AnimationParser.Core.Shapes;
using AnimationParser.Core.Commands;
using System.Diagnostics;

namespace AnimationParser.Game;

public partial class AnimationScene : Container
{
    private class InterpreterContext : AnimationContext
    {
        // Containing Drawables that interacts with the Game Engine
        // In this case, osu!framework. But can be easily to adapt to other engines if you implement this context
        private Dictionary<string, Drawable> drawables = new();

        private AnimationScene container;
        public InterpreterContext(AnimationScene container)
        {
            this.container = container;
        }

        const int duration = 500;

        protected override void OnObjectAdded(string name, AnimationObject obj)
        {
            // Add a associated binding GameObject to the Game Engine
            var drawable = new Container()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.CentreLeft,
            };

            foreach (var shapeBase in obj.Shapes)
            {
                switch (shapeBase.Type)
                {
                    case ShapeType.Line:
                        {
                            var shape = (LineShape)shapeBase;
                            var vector = shape.End - shape.Start;
                            var length = vector.Length();

                            var rotation = MathHelper.RadiansToDegrees((float)Math.Atan2(vector.Y, vector.X));
                            var position = new Vector2(shape.Start.X, shape.Start.Y);

                            drawable.Add(new Box()
                            {
                                RelativeSizeAxes = Axes.None,
                                RelativePositionAxes = Axes.None,
                                Origin = Anchor.CentreLeft,
                                Height = 3.0f,
                                Width = length,
                                Rotation = rotation,
                                Position = position,
                            });
                            break;
                        }

                    case ShapeType.Circle:
                        {
                            var shape = (CircleShape)shapeBase;
                            drawable.Add(new Circle()
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(shape.Radius * 2),
                                Position = new Vector2(shape.Center.X, shape.Center.Y), // We have to cast to OpenTk.Vector2
                            });
                        }
                        break;
                }
            }

            base.OnObjectAdded(name, obj);
            drawables.Add(name, drawable);
            container.Add(drawable);

            drawable.FlashColour(Color4.Pink, 800, Easing.OutQuint)
                .OnComplete(_ => container.MoveNextCommand());
        }

        protected override void OnObjectErasingOut(string name, AnimationObject obj)
        {
            if (drawables.TryGetValue(name, out var drawable))
            {
                var delay = drawable.Transforms.Select(t => t.EndTime - drawable.Time.Current)
                    .OrderDescending()
                    .FirstOrDefault();

                drawable.Delay(delay).Then().FadeOut(duration).Expire()
                    .OnComplete(_ =>
                    {
                        drawables.Remove(name);
                        base.OnObjectErasingOut(name, obj);
                        container.MoveNextCommand();
                    });
            }
        }

        protected override void OnObjectPlaced(string name, AnimationObject obj, System.Numerics.Vector2 position)
        {
            if (drawables.TryGetValue(name, out var drawable))
            {
                var delay = drawable.Transforms.Select(t => t.EndTime - drawable.Time.Current)
                    .OrderDescending()
                    .FirstOrDefault();

                drawable.Delay(delay)
                    .Then()
                    .MoveTo(new Vector2(position.X, position.Y), duration)
                    .OnComplete(_ =>
                    {
                        base.OnObjectPlaced(name, obj, position);
                        container.MoveNextCommand();
                    });
            }
        }

        protected override void OnObjectShifting(string name, AnimationObject obj, Core.Direction direction)
        {
            base.OnObjectShifting(name, obj, direction);
            var rect = container.DrawRectangle;

            if (drawables.TryGetValue(name, out var drawable))
            {
                var delay = drawable.Transforms.Select(t => t.EndTime - drawable.Time.Current)
                    .OrderDescending()
                    .FirstOrDefault();

                var transfrom = drawable.Delay(delay)
                    .Then();

                transfrom = direction switch
                {
                    Core.Direction.Left => transfrom.MoveToX(-container.DrawWidth / 2, duration),
                    Core.Direction.Right => transfrom.MoveToX(container.DrawWidth / 2, duration),
                    Core.Direction.Up => transfrom.MoveToY(-container.DrawHeight / 2, duration),
                    Core.Direction.Down => transfrom.MoveToY(container.DrawHeight / 2, duration),
                    _ => throw new ArgumentException($"Giving direction is not valid: {direction}"),
                };

                transfrom.OnComplete(_ => container.MoveNextCommand());
            }
        }
    }

    private InterpreterContext context;

    public AnimationScene()
    {
        context = new InterpreterContext(this);
        RelativeSizeAxes = Axes.Both;
    }

    public new void Clear()
    {
        base.Clear();
        context = new InterpreterContext(this);
    }

    private IEnumerator<IAnimationCommand>? commandEnumerator;
    private void MoveNextCommand()
    {
        Debug.Assert(commandEnumerator is not null);

        if (commandEnumerator.MoveNext())
        {
            commandEnumerator.Current.Execute(context);
        }
    }

    public void ExecuteAnimationCommands(IEnumerable<IAnimationCommand> commands)
    {
        Clear();

        // Flatten the commands to avoid executing whole loops at once.
        commandEnumerator = commands.Flatten().GetEnumerator();
        MoveNextCommand();
    }
}
