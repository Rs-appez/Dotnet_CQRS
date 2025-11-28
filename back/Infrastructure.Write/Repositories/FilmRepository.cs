using Npgsql;
using Domain.Entities;
using Application.Common.Interfaces;

namespace Infrastructure.Write.Repositories;

public class FilmWriteRepository(string connectionString) : IFilmWriteRepository
{
    private readonly string _connectionString = connectionString;

    public async Task<int> AddFilm(Film film, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var command = new NpgsqlCommand("INSERT INTO film (fr_name, qc_name, year) VALUES (@fr_name, @qc_name, @year) RETURNING id", connection);
        command.Parameters.AddWithValue("fr_name", film.Fr_Title);
        command.Parameters.AddWithValue("qc_name", film.Qc_Title);
        command.Parameters.AddWithValue("year", film.Release_Year);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        film.Film_Id = Convert.ToInt32(result);

        return film.Film_Id;
    }

}
