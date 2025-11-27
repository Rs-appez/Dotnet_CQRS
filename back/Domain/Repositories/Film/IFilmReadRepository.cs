using Domain.Entities;

namespace Domain.Repositories.FilmRead;

public interface IFilmReadRepository
{
    
    Task<IEnumerable<Film>> GetAllFilms(CancellationToken cancellationToken = default);
}
