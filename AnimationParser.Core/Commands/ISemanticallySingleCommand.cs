namespace AnimationParser.Core.Commands;

public interface ISemanticallySingleCommand : IAnimationCommand
{
    void Execute(AnimationContext context);
}
