using Domain.Entities;

namespace Domain.Repositories.FilmWrite;

public interface IFilmWriteRepository
{
    Task<Film> AddFilm(Film film, CancellationToken cancellationToken = default);
}
