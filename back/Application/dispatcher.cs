using Application.Abstractions;
namespace Application.Dispatching;

public class Dispatcher(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public TResult ExecuteQuery<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(typeof(TQuery), typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType);
        var method = handlerType.GetMethod("Execute");
        return (TResult)method.Invoke(handler, new object[] { query });
    }

    public TResult ExecuteCommand<TCommand, TResult>(TCommand command) where TCommand : ICommand<TResult>
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(typeof(TCommand), typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType);
        var method = handlerType.GetMethod("Execute");
        return (TResult)method.Invoke(handler, new object[] { command });
    }
}
