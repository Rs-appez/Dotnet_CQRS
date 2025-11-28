using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
namespace Application.Films.Create;

public class CreateFilmHandler(IFilmWriteRepository filmRepository) : IRequestHandler<CreateFilmCommand, int>
{
    private readonly IFilmWriteRepository _filmRepository = filmRepository;

    public async Task<int> Handle(CreateFilmCommand command, CancellationToken cancellationToken)
    {
        var film = new Film
        (
         command.FrTitle,
         command.QcTitle,
         command.ReleaseYear
        );

        await _filmRepository.AddFilm(film, cancellationToken);
        return film.Film_Id;
    }
}
