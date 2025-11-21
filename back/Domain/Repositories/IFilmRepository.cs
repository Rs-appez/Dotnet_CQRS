using Domain.Entities;

namespace Domain.Repositories;

public interface IFilmRepository
{
    Task<IEnumerable<Film>> GetAllFilms(CancellationToken cancellationToken = default);
}
