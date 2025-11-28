using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IFilmWriteRepository
{
    Task<int> AddFilm(Film film, CancellationToken cancellationToken = default);
}
