using Application.Films;

namespace Application.Common.Interfaces;

public interface IFilmReadRepository
{
    
    Task<IEnumerable<FilmDto>> GetAllFilms(CancellationToken cancellationToken = default);
}
