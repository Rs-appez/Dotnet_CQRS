using MongoDB.Driver;
using Application.Common.Interfaces;
using Application.Films;

namespace Infrastructure.ReadModel;

public class FilmReadRepository(IMongoDatabase database) : IFilmReadRepository
{
    private IMongoCollection<ReadFilm> Films => database.GetCollection<ReadFilm>("film");

    public async Task<IEnumerable<FilmDto>> GetAllFilms(CancellationToken cancellationToken = default)
    {
        var films = await Films.Find(_ => true).ToListAsync(cancellationToken);
        return films;
    }
}
