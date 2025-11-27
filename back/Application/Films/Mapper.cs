using Domain.Entities;
namespace Application.Films;

public static class Mapper
{
    public static FilmDto ToDto(this Film film)
    {
        return new FilmDto(film.Film_Id, film.Fr_Title, film.Qc_Title, film.Release_Year);
    }
}
