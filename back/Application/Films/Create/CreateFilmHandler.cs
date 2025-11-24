using Domain.Repositories.FilmWrite;
using Domain.Entities;
using MediatR;
namespace Application.Films.Create;

public class CreateFilmHandler(IFilmWriteRepository filmRepository) : IRequestHandler<CreateFilmCommand, Film>
{
    private readonly IFilmWriteRepository _filmRepository = filmRepository;

    public async Task<Film> Handle(CreateFilmCommand command, CancellationToken cancellationToken)
    {
        var film = new Film
        (
         command.FrTitle,
         command.QcTitle,
         command.ReleaseYear
        );

        await _filmRepository.AddFilm(film, cancellationToken);
        return film;
    }
}
