using MediatR;
using Domain.Repositories;

namespace Application.Films.GetAll;

public record GetAllFilmsQuery() : IRequest<IReadOnlyList<FilmDto>>;

public class GetAllFilmsHandler(IFilmRepository filmRepository) : IRequestHandler<GetAllFilmsQuery, IReadOnlyList<FilmDto>>
{
    private readonly IFilmRepository _filmRepository = filmRepository;

    public async Task<IReadOnlyList<FilmDto>> Handle(GetAllFilmsQuery _, CancellationToken cancellationToken)
    {
        var films = await _filmRepository.GetAllFilms(cancellationToken);
        return [.. films.Select(film => film.ToDto())];
    }
}
