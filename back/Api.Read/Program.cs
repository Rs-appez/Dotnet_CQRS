using MediatR;
using Infrastructure.Read.Repositories;
using Domain.Repositories.FilmRead;
using Application.Films.GetAll;
using Application.Films;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

string connectionString = builder.Configuration.GetConnectionString("default") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddScoped<IFilmReadRepository>(_ => new FilmReadRepository(connectionString));
builder.Services.AddTransient<IRequestHandler<GetAllFilmsQuery, IReadOnlyList<FilmDto>>, GetAllFilmsHandler>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
