using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Films;
using Application.Films.Create;

namespace Api.Write.Controllers;

[ApiController]
[Route("[controller]")]
public class FilmController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    public record CreateFilmRequest(string FrTitle, string QcTitle, int ReleaseYear);

    [HttpPost(Name = "CreateFilm")]
    public ActionResult<FilmDto> Create([FromBody] CreateFilmRequest request, CancellationToken ct)
    {
        CreateFilmCommand command = new(request.FrTitle, request.QcTitle, request.ReleaseYear);
        FilmDto film = _mediator.Send(command, ct).Result.ToDto();
        return film;
    }
}
