namespace Application.Abstractions;

public interface IQuery<out TResult>
{
}

public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    TResult Execute(TQuery query);
}

