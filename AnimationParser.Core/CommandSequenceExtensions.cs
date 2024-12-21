using System.Diagnostics;
using AnimationParser.Core.Commands;
using AnimationParser.Core.Tokens;

namespace AnimationParser.Core;

public static class CommandSequenceExtensions
{
    private abstract class LoopFrame
    {
        public LoopFrame(IEnumerable<IAnimationCommand> commandSequence)
        {
            SequenceEnumerator = commandSequence.GetEnumerator();
        }

        /// <summary>
        /// Indicates if the loop has finished.
        /// </summary>
        public abstract bool IsFinished { get; }

        /// <summary>
        /// Marks the end of an iteration of the loop.
        /// </summary>
        /// <returns>If the loop should continue.</returns>
        public abstract bool OnIterationEnd();

        public IEnumerator<IAnimationCommand> SequenceEnumerator { get; private set; } = null!;
    }

    private class InitialFrame : LoopFrame
    {
        public InitialFrame(IEnumerable<IAnimationCommand> commandSequence)
            : base(commandSequence)
        {
        }

        public override bool IsFinished => false;
        public override bool OnIterationEnd() => false;
    }

    private class CountedLoopFrame : LoopFrame
    {
        public CountedLoopFrame(LoopCommand loopCommand)
            : base(loopCommand.Commands)
        {
            RemainingIterations = loopCommand.Count;
        }

        public override bool IsFinished => RemainingIterations <= 0;

        public int RemainingIterations { get; set; }

        public override bool OnIterationEnd()
            => --RemainingIterations > 0;
    }

    /// <summary>
    /// Flattens the sequence of commands with loops into a single sequence of commands.
    /// </summary>
    /// <param name="commands">The sequence of commands to flatten.</param>
    /// <returns>The flattened sequence of commands.</returns>
    public static IEnumerable<ISemanticallySingleCommand> Flatten(this IEnumerable<IAnimationCommand> commands)
    {
        var loopFrames = new Stack<LoopFrame>();
        loopFrames.Push(new InitialFrame(commands));

        while (loopFrames.TryPeek(out var currentFrame))
        {
            Debug.Assert(currentFrame != null);
            Debug.Assert(currentFrame is InitialFrame || !currentFrame.IsFinished);

            if (currentFrame.SequenceEnumerator.MoveNext())
            {
                switch (currentFrame.SequenceEnumerator.Current)
                {
                    case LoopCommand loopCommand when loopCommand.Count > 0:
                        loopFrames.Push(new CountedLoopFrame(loopCommand));
                        break;

                    case LoopCommand:
                        break;

                    // Prevent null commands
                    case ISemanticallySingleCommand animationCommand:
                        yield return animationCommand;
                        break;

                    default:
                        // The switch statement generates IL as below
                        // IAnimationCommand animationCommand = currentFrame.SequenceEnumerator.Current;
                        // LoopCommand loopCommand = animationCommand as LoopCommand;
                        // if (loopCommand is not null)
                        // {
                        //     if (loopCommand.Count > 0)
                        //         ...
                        // }
                        // else
                        // {
                        //     if (animationCommand is not null)
                        //     {
                        //          yield return animation command;
                        //     }
                        //     else
                        //     {
                        //          throw new InvalidOperationException("Command can NOT be null!");
                        //     }
                        // }
                        throw new InvalidOperationException("Command can NOT be null!");
                }
            }
            else
            {
                if (currentFrame.OnIterationEnd())
                {
                    // Reset the enumerator to start the next iteration
                    currentFrame.SequenceEnumerator.Reset();
                }
                else
                {
                    loopFrames.Pop();
                }
            }
        }
    }

    /// <summary>
    /// Interprets the source code into a sequence of commands.
    /// Ensure that every returned command does not involve implicit multiple commands.
    /// </summary>
    /// <param name="source">The source code to interpret.</param>
    /// <returns>The sequence of commands at the execution order.</returns>
    public static IEnumerable<ISemanticallySingleCommand> InterpretExecutable(this IEnumerable<Token> tokenStream)
    {
        var parser = new Parser(tokenStream);
        return parser.Parse().Flatten();
    }

    /// <summary>
    /// Interprets and executes the source code synchronously.
    /// </summary>
    /// <param name="tokenStream">The tokenized source code.</param>
    /// <param name="context">The context to execute the commands.</param>
    public static void InterprettedlyExecuteAll(this IEnumerable<Token> tokenStream, AnimationContext? context = null)
    {
        context ??= new AnimationContext();

        foreach (var command in tokenStream.InterpretExecutable())
        {
            command.Execute(context);
        }
    }

    /// <summary>
    /// Interprets and executes the source code asynchronously. Every time a command is executed, it yields the control to the caller.
    /// </summary>
    /// <param name="tokenStream">The tokenized source code.</param>
    /// <param name="context">The context to execute the commands.</param>
    public static async Task InterprettedlyExecuteAllAsync(this IEnumerable<Token> tokenStream, AnimationContext? context = null)
    {
        context ??= new AnimationContext();

        foreach (var command in tokenStream.InterpretExecutable())
        {
            command.Execute(context);
            await Task.Yield();
        }
    }
}
