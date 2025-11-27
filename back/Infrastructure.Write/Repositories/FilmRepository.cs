using Npgsql;
using Domain.Entities;
using Domain.Repositories.FilmWrite;

namespace Infrastructure.Write.Repositories;

public class FilmWriteRepository(string connectionString) : IFilmWriteRepository
{
    private readonly string _connectionString = connectionString;

    public async Task<Film> AddFilm(Film film, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = new NpgsqlCommand("INSERT INTO film (fr_name, qc_name, year) VALUES (@fr_name, @qc_name, @year) RETURNING id", connection);
        command.Parameters.AddWithValue("fr_name", film.Fr_Title);
        command.Parameters.AddWithValue("qc_name", film.Qc_Title);
        command.Parameters.AddWithValue("year", film.Release_Year);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        film.Film_Id = Convert.ToInt32(result);

        var commandCheck = new NpgsqlCommand("SELECT id, fr_name, qc_name, year from film WHERE id = @id", connection);
        commandCheck.Parameters.AddWithValue("id", film.Film_Id);
        await using var reader = await commandCheck.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))

            film = new Film
            (
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3)
            );

        return film;
    }

}
