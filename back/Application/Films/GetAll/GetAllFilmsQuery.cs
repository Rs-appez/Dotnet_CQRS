using MediatR;

namespace Application.Films.GetAll;

public record GetAllFilmsQuery() : IRequest<IReadOnlyList<FilmDto>>;
