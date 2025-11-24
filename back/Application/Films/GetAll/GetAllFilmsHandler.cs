using MediatR;
using Domain.Repositories.FilmRead;

namespace Application.Films.GetAll;


public class GetAllFilmsHandler(IFilmReadRepository filmRepository) : IRequestHandler<GetAllFilmsQuery, IReadOnlyList<FilmDto>>
{
    private readonly IFilmReadRepository _filmRepository = filmRepository;

    public async Task<IReadOnlyList<FilmDto>> Handle(GetAllFilmsQuery _, CancellationToken cancellationToken)
    {
        var films = await _filmRepository.GetAllFilms(cancellationToken);
        return [.. films.Select(film => film.ToDto())];
    }
}
