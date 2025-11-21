using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.Films.GetAll;
using Application.Films;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class FilmController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet(Name = "GetFilms")]
    public IEnumerable<FilmDto> Get(CancellationToken ct)
    {
        IEnumerable<FilmDto> films = _mediator.Send(new GetAllFilmsQuery(), ct).Result;
        return films;
    }
}
