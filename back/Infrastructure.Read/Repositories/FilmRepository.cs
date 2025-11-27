using MongoDB.Driver;
using Domain.Repositories.FilmRead;
using Domain.Entities;
using Infrastructure.Read.Models;

namespace Infrastructure.Read.Repositories;

public class FilmReadRepository(IMongoDatabase database) : IFilmReadRepository
{
    private IMongoCollection<ReadFilm> Films => database.GetCollection<ReadFilm>("film");

    public async Task<IEnumerable<Film>> GetAllFilms(CancellationToken cancellationToken = default)
    {
        return await Films
            .Find(FilterDefinition<ReadFilm>.Empty)
            .ToListAsync(cancellationToken);
    }
}
