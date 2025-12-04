using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
namespace Application.Films.Create;

public class CreateFilmHandler(IFilmWriteRepository filmRepository, IMessageBus messageBus) : IRequestHandler<CreateFilmCommand, int>
{
    private readonly IFilmWriteRepository _filmRepository = filmRepository;
    private readonly IMessageBus _messageBus = messageBus;

    public async Task<int> Handle(CreateFilmCommand command, CancellationToken cancellationToken)
    {
        var film = new Film
        (
         command.FrTitle,
         command.QcTitle,
         command.ReleaseYear
        );

        await _filmRepository.AddFilm(film, cancellationToken);
        await _messageBus.PublishAsync(new FilmDto(film.Film_Id, film.Fr_Title, film.Qc_Title, film.Release_Year),
                routingKey: "order.created",
                cancellationToken);
        return film.Film_Id;
    }
}
