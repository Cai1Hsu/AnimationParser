using System.Numerics;
using System.Runtime.CompilerServices;
using AnimationParser.Core.Commands;
using AnimationParser.Core.Shapes;
using AnimationParser.Core.Tokens;

namespace AnimationParser.Core;

/// <summary>
/// The parser is responsible for parsing the tokens into a sequence of animation commands.
/// This class is based on a recursive descent parser, which is a top-down parser that
/// starts from the root of the syntax tree and works its way down to the leaves.
/// It's implemented using a visitor pattern, where each method is responsible for
/// parsing a specific command or expression.
/// This class is also using stackless coroutines to avoid the overhead of creating
/// a new stack frame for each recursive call, meaning it's more efficient than
/// parse all the tokens at once.
/// </summary>
public class Parser
{
    private IEnumerable<Token> Tokens { get; }

    private IEnumerator<Token> TokenEnumerator { get; }

    public Token CurrentToken => TokenEnumerator.Current;

    private bool hasToken = true;
    public bool HasToken => hasToken;

    private Token? MoveNext()
    {
        if (!HasToken || !(hasToken = TokenEnumerator.MoveNext()))
            return null!;

        return CurrentToken;
    }

    public Parser(IEnumerable<Token> tokens)
    {
        Tokens = tokens;
        TokenEnumerator = Tokens.GetEnumerator();
    }

    /// <summary>
    /// Parses the tokens into a sequence of animation commands. The returned
    /// sequence is lazy evaluated and faithfully represents the input source.
    /// To Get the execution sequence, use <see cref="CommandSequenceExtensions.Flatten"/>,
    /// which flatten the loop commands into a single sequence of commands.
    /// </summary>
    /// <returns>A sequence of animation commands faithfully representing the input source</returns>
    public IEnumerable<IAnimationCommand> Parse()
    {
        while (HasToken)
        {
            MoveNextIsNotNull();

            if (CurrentToken.Type is TokenType.EndOfSource)
                break;

            Assert(CurrentToken.Type is TokenType.LeftParen, "Expected '('");

            yield return VisitCommand();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // Inlining this method to avoid garbage stack trace information
    private void Assert(bool predicate, string message)
    {
        if (!predicate)
        {
            throw new Exception($"{message}. Current token: {CurrentToken}");
        }
    }

    public void MoveNextIsNotNull()
    {
        Assert(HasToken, "Unexpected end of input");
        Assert(MoveNext() is not null, "Unexpected end of input");
    }

    /// <summary>
    /// Visits a command by lazing evaluating the tokens into a command.
    /// Can be one of the following commands:
    /// - DefineCommand
    /// - PlaceCommand
    /// - ShiftCommand
    /// - EraseCommand
    /// - LoopCommand
    /// *Leading left parenthesis IS expected to be consumed BEFORE calling this method.
    /// *Trailing right parenthesis IS expected to be consumed IN this method.
    /// </summary>
    /// <returns>The visited command</returns>
    /// <exception cref="Exception">Syntax error or unexpected command</exception>
    public IAnimationCommand VisitCommand()
    {
        MoveNextIsNotNull();
        var commandToken = CurrentToken;

        Assert(commandToken.Type is TokenType.Keyword, $"Expected command keyword, but got: {commandToken}");

        var command = commandToken.Text switch
        {
            "define" => (IAnimationCommand)VisitDefineCommand(),
            "place" => (IAnimationCommand)VisitPlaceCommand(),
            "shift" => (IAnimationCommand)VisitShiftCommand(),
            "erase" => (IAnimationCommand)VisitEraseCommand(),
            "loop" => (IAnimationCommand)VisitLoopCommand(),
            _ => throw new Exception($"Unexpected command: {commandToken.Text}")
        };

        return command;
    }

    /// <summary>
    /// Visits a draw list by lazing evaluating the tokens into a sequence of shapes.
    /// A draw list is a sequence of shapes that can be drawn on the screen.
    /// *Leading left parenthesis is NOT expected to be consumed IN this method.
    /// *Trailing right parenthesis IS expected to be consumed IN this method.
    /// </summary>
    /// <returns>A sequence of shapes</returns>
    /// <exception cref="Exception">Mainly Syntax error</exception>
    public AnimationObject VisitDrawList()
    {
        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.LeftParen, "Expected '('");

        var obj = new AnimationObject();

        while (HasToken)
        {
            MoveNextIsNotNull();

            switch (CurrentToken.Type)
            {
                case TokenType.RightParen: // end of draw list
                    return obj;

                case TokenType.LeftParen:
                    obj.AddShape(VisitShapeCommand());
                    break;

                default:
                    throw new Exception($"Expected '(' but got: {CurrentToken}");
            }
        }

        throw new Exception("Unclosed draw list");
    }

    /// <summary>
    /// Visits a shape command by lazing evaluating the tokens into a shape.
    /// Can be one of the following shapes:
    /// - CircleShape
    /// - LineShape
    /// *Leading left parenthesis IS expected to be consumed BEFORE calling this method.
    /// *Trailing right parenthesis IS expected to be consumed IN this method.
    /// </summary>
    /// <returns>The visited shape</returns>
    /// <exception cref="Exception">Syntax error or unexpected shape</exception>
    public IShape VisitShapeCommand()
    {
        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.Keyword, "Expected keyword");

        var shape = CurrentToken.Text! switch
        {
            "circle" => (IShape)VisitCircleShape(),
            "line" => (IShape)VisitLineShape(),
            _ => throw new Exception($"Unexpected shape command: {CurrentToken.Text}")
        };

        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.RightParen, "Expected ')'"); // close shape command

        return shape;
    }

    /// <summary>
    /// Visits a number by lazing evaluating the tokens into a number.
    /// No leading or trailing parenthesis is expected for this method.
    /// </summary>
    /// <returns>The visited number</returns>
    /// <exception cref="Exception">syntax error or number parse error</exception>
    public float VisitNumber()
    {
        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.Number, "Expected number");

        if (!float.TryParse(CurrentToken.Text, out var number))
        {
            throw new Exception($"Expected number but got: {CurrentToken}");
        }

        return number;
    }

    /// <summary>
    /// Visits a circle shape by lazing evaluating the tokens into a circle shape.
    /// *Leading left parenthesis IS expected to be consumed before calling this method.
    /// *Trailing right parenthesis IS expected to be consumed after calling this method.
    /// </summary>
    /// <returns>The visited circle shape</returns>
    public CircleShape VisitCircleShape()
    {
        return new CircleShape(VisitVector2(), VisitNumber());
    }

    /// <summary>
    /// Visits a line shape by lazing evaluating the tokens into a line shape.
    /// *Leading left parenthesis IS expected to be consumed before calling this method.
    /// *Trailing right parenthesis IS expected to be consumed after calling this method.
    /// </summary>
    /// <returns>The visited line shape</returns>
    public LineShape VisitLineShape()
    {
        return new LineShape(VisitVector2(), VisitVector2());
    }

    /// <summary>
    /// Visits a vector2 by lazing evaluating the tokens into a vector2.
    /// *Leading left parenthesis IS expected to be consumed IN this method.
    /// *Trailing right parenthesis IS expected to be consumed IN this method.
    /// </summary>
    /// <returns>The visited vector2</returns>
    public Vector2 VisitVector2()
    {
        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.LeftParen, "Expected '('");

        var x = VisitNumber();
        var y = VisitNumber();

        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.RightParen, "Expected ')'");

        return new Vector2(x, y);
    }

    /// <summary>
    /// Visits a direction by lazing evaluating the tokens into a direction.
    /// No leading or trailing parenthesis is expected for this method.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Direction VisitDirection()
    {
        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.Keyword, "Expected keyword");

        return CurrentToken.Text switch
        {
            "up" => Direction.Up,
            "down" => Direction.Down,
            "left" => Direction.Left,
            "right" => Direction.Right,
            _ => throw new Exception($"Unexpected direction: {CurrentToken.Text}")
        };
    }

    /// <summary>
    /// Visits a define command by lazing evaluating the tokens into a define command.
    /// A define command is a command that defines a named object with a draw list.
    /// Follows systax: (define <name> <draw-list>)
    /// *Leading left parenthesis IS expected to be consumed BEFORE calling this method.
    /// *Trailing right parenthesis IS expected to be consumed IN this method.
    /// </summary>
    /// <returns>The visited define command</returns>
    public DefineCommand VisitDefineCommand()
    {
        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.Identifier, "Expected identifier");

        var objName = CurrentToken.Text;
        Assert(objName.Length > 0, "Name of an object can not be empty");

        var obj = VisitDrawList();

        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.RightParen, "Expected ')'"); // close command

        return new DefineCommand(objName.ToString(), obj);
    }

    /// <summary>
    /// Visits a place command by lazing evaluating the tokens into a place command.
    /// A place command is a command that places a named object at a specific position.
    /// Follows syntax: (place <name> (<Vector2>))
    /// *Leading left parenthesis IS expected to be consumed BEFORE calling this method.
    /// *Trailing right parenthesis IS expected to be consumed IN this method.
    /// </summary>
    /// <returns>The visited place command</returns>
    public PlaceCommand VisitPlaceCommand()
    {
        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.Identifier, "Expected identifier");

        var objName = CurrentToken.Text;
        Assert(objName.Length > 0, "Object name can not be empty");

        var position = VisitVector2();

        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.RightParen, "Expected ')'"); // close command

        return new PlaceCommand(objName.ToString(), position);
    }

    /// <summary>
    /// Visits a shift command by lazing evaluating the tokens into a shift command.
    /// A shift command is a command that shifts a named object in a specific direction.
    /// Follows syntax: (shift <name> <direction>)
    /// *Leading left parenthesis IS expected to be consumed BEFORE calling this method.
    /// *Trailing right parenthesis IS expected to be consumed IN this method.
    /// </summary>
    /// <returns>The visited shift command</returns>
    public ShiftCommand VisitShiftCommand()
    {
        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.Identifier, "Expected identifier");

        var objName = CurrentToken.Text;
        Assert(objName.Length > 0, "Name of an object can not be empty");

        var direction = VisitDirection();

        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.RightParen, "Expected ')'"); // close command

        return new ShiftCommand(objName.ToString(), direction);
    }

    /// <summary>
    /// Visits an erase command by lazing evaluating the tokens into an erase command.
    /// An erase command is a command that erases a named object.
    /// Follows syntax: (erase <name>)
    /// *Leading left parenthesis IS expected to be consumed BEFORE calling this method.
    /// *Trailing right parenthesis IS expected to be consumed IN this method.
    /// </summary>
    /// <returns>The visited erase command</returns>
    public EraseCommand VisitEraseCommand()
    {
        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.Identifier, "Expected identifier");

        var objName = CurrentToken.Text!;
        Assert(objName.Length > 0, "Name of an object can not be empty");

        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.RightParen, "Expected ')'"); // close command

        return new EraseCommand(objName.ToString());
    }

    /// <summary>
    /// Visits a loop command by lazing evaluating the tokens into a loop command.
    /// A loop command is a command that repeats a sequence of commands a specific number of times.
    /// Follows syntax: (loop <count> (<command> ...))
    /// *Leading left parenthesis IS expected to be consumed BEFORE calling this method.
    /// *Trailing right parenthesis IS expected to be consumed IN this method.
    /// </summary>
    /// <returns>The visited loop command</returns>
    public LoopCommand VisitLoopCommand()
    {
        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.Number, "Expected number");

        int count = int.Parse(CurrentToken.Text!);

        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.LeftParen, "Expected '('"); // begin loop commands

        var commands = new List<IAnimationCommand>();

        while (HasToken)
        {
            MoveNextIsNotNull();
            if (CurrentToken.Type is TokenType.RightParen) // close the list of commands
                break;

            commands.Add(VisitCommand());
        }

        MoveNextIsNotNull();
        Assert(CurrentToken.Type is TokenType.RightParen, "Expected ')'"); // close command

        return new LoopCommand(count, commands);
    }
}
