namespace Application.Abstractions;

public interface ICommand<out TResult>
{
}

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    TResult Execute(TCommand command);
}
