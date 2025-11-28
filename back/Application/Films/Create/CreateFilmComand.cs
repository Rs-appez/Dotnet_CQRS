using MediatR;
using Domain.Entities;
namespace Application.Films.Create;

public record CreateFilmCommand(string FrTitle, string QcTitle, int ReleaseYear) : IRequest<int>;
