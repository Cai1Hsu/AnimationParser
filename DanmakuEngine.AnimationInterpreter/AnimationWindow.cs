using System.Diagnostics;
using System.Numerics;
using AnimationParser.Core;
using AnimationParser.Core.Commands;
using AnimationParser.Core.Shapes;
using DanmakuEngine.DearImgui.Windowing;
using DanmakuEngine.Timing;
using ImGuiNET;

namespace DanmakuEngine.AnimationInterpreter;

public class AnimationWindow : ImguiWindowBase
{
    private ImGuiAnimationContext context = null!;

    private class ImGuiAnimationContext : AnimationContext
    {
        private AnimationWindow window;
        public ImGuiAnimationContext(AnimationWindow window)
        {
            this.window = window;
        }

        private void SetCurrentCoroutine(IEnumerable<bool> coroutine)
        {
            CoroutineEnumerator = coroutine.GetEnumerator();
        }

        private IEnumerator<bool>? CoroutineEnumerator;
        public bool MoveNext()
        {
            if (CoroutineEnumerator is not null)
            {
                if (CoroutineEnumerator.MoveNext())
                {
                    return true;
                }
                else
                {
                    return window.MoveNextCommand();
                }
            }

            return false;
        }

        private Dictionary<string, ObjectInfo> objectsInfo = new Dictionary<string, ObjectInfo>();

        public class ObjectInfo
        {
            public float Alpha { get; set; }
        }

        public new (AnimationObject, ObjectInfo) GetObject(string name)
        {
            return (AnimatedObjects[name], objectsInfo[name]);
        }

        const float duration = 1f;

        protected override void OnObjectAdded(string name, AnimationObject drawable)
        {
            SetCurrentCoroutine(FadeIn());

            IEnumerable<bool> FadeIn()
            {
                objectsInfo.Add(name, new ObjectInfo());
                base.OnObjectAdded(name, drawable);

                var info = objectsInfo[name];
                var start = Time.ElapsedSeconds;

                while (Time.ElapsedSeconds - start < duration)
                {
                    info.Alpha = (float)(Time.ElapsedSeconds - start) / duration;
                    yield return true;
                }

                info.Alpha = 1;
            }
        }

        protected override void OnObjectErasingOut(string name, AnimationObject drawable)
        {
            SetCurrentCoroutine(FadeOut());

            IEnumerable<bool> FadeOut()
            {
                var info = objectsInfo[name];
                var start = Time.ElapsedSeconds;

                while (Time.ElapsedSeconds - start < duration)
                {
                    info.Alpha = 1 - (float)(Time.ElapsedSeconds - start) / duration;
                    yield return true;
                }

                base.OnObjectErasingOut(name, drawable);
            }
        }

        protected override void OnObjectPlaced(string name, AnimationObject obj, Vector2 position)
        {
            SetCurrentCoroutine(PlaceObject());

            IEnumerable<bool> PlaceObject()
            {
                var start = Time.ElapsedSeconds;
                var startPosition = obj.Position;

                while (Time.ElapsedSeconds - start < duration)
                {
                    obj.Position = startPosition + (position - startPosition) * (float)(Time.ElapsedSeconds - start) / duration;
                    yield return true;
                }

                base.OnObjectPlaced(name, obj, position);
            }
        }

        protected override void OnObjectShifting(string name, AnimationObject drawable, Direction direction)
        {
            SetCurrentCoroutine(ShiftObject());

            IEnumerable<bool> ShiftObject()
            {
                base.OnObjectShifting(name, drawable, direction);
                var start = Time.ElapsedSeconds;
                var startPosition = drawable.Position;

                Vector2 destination = direction switch
                {
                    Direction.Up => new Vector2(drawable.Position.X, 0),
                    Direction.Down => new Vector2(drawable.Position.X, 400),
                    Direction.Left => new Vector2(0, drawable.Position.Y),
                    Direction.Right => new Vector2(400, drawable.Position.Y),
                    _ => throw new NotImplementedException()
                };

                while (Time.ElapsedSeconds - start < duration)
                {
                    var diff = destination - startPosition;
                    drawable.Position = startPosition + diff * (float)(Time.ElapsedSeconds - start) / duration;
                    yield return true;
                }

                base.OnObjectShifting(name, drawable, direction);
            }
        }
    }

    public AnimationWindow()
        : base("Animation Demo Window")
    {
    }

    private IEnumerator<ISemanticallySingleCommand>? commandEnumerator;
    private bool MoveNextCommand()
    {
        Debug.Assert(commandEnumerator is not null);

        if (commandEnumerator.MoveNext())
        {
            commandEnumerator.Current.Execute(context);
            return true;
        }

        return false;
    }

    const string defaultCode = """
                               (define cross ( (line (0 0) (50 50)) (line (50 0) (0 50)) (circle (25 25) 25) ) )
                               (place cross (50 50))
                               (loop 5 ( (shift cross right) (shift cross up) (shift cross left) (shift cross down)) )
                               (erase cross)
                               """;

    private string codeBuffer = defaultCode;
    private string? errorMessage = null;
    protected override void Update()
    {
        ImGui.Text("Paste your animation code here:");
        ImGui.InputTextMultiline("##AnimationCode", ref codeBuffer, 4096, new Vector2(800, 200));

        if (ImGui.Button("Run Animation"))
        {
            try
            {
                var lexer = new Lexer(codeBuffer);
                var parser = new Parser(lexer.Tokenize());

                context = new ImGuiAnimationContext(this);

                commandEnumerator = parser.Parse().Flatten().GetEnumerator();
                MoveNextCommand();

                errorMessage = null;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
        }

        if (errorMessage != null)
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), errorMessage);
        }

        if (context != null)
        {
            try
            {
                context.MoveNext();
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }

            var drawList = ImGui.GetWindowDrawList();
            var windowPos = ImGui.GetWindowPos();
            var titleBarHeight = ImGui.GetFrameHeight();

            // Draw objects
            foreach (var (drawable, info) in context.AnimatedObjects.Select(x => context.GetObject(x.Key)))
            {
                Vector4 color4 = new Vector4(Vector3.One, info.Alpha);
                var color = ImGui.GetColorU32(color4);

                // No scaling and rotation factor, we can apply the transformation directly
                var parentPosition = drawable.Position + windowPos + new Vector2(0, titleBarHeight);
                foreach (var shape in drawable.Shapes)
                {
                    if (shape is null)
                        continue;

                    switch (shape)
                    {
                        case CircleShape circle:
                            var position = parentPosition + circle.Center;
                            drawList.AddCircleFilled(position, circle.Radius, color);
                            break;

                        case LineShape line:
                            var start = parentPosition + line.Start;
                            var end = parentPosition + line.End;
                            drawList.AddLine(start, end, color);
                            break;
                    }
                }
            }
        }
    }
}