using Npgsql;
using Domain.Entities;
using Domain.Repositories;

namespace Infrastructure.Repositories;

public class FilmRepository(string connectionString) : IFilmRepository
{
    private readonly string _connectionString = connectionString;

    public async Task<IEnumerable<Film>> GetAllFilms(CancellationToken cancellationToken = default)
    {
        var films = new List<Film>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = new NpgsqlCommand("SELECT id, fr_name, qc_name, year from film", connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            films.Add(new Film
            (
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3)
            ));

        }
        return films;
    }
}
