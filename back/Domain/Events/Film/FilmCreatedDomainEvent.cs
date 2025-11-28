namespace Domain.Events.Film;

public sealed class FilmCreatedDomainEvent(int filmId, string fr_title, string qc_title, int release_year) : IDomainEvent
{
    public int FilmId { get; } = filmId;
    public string Fr_Title { get; } = fr_title;
    public string Qc_Title { get; } = qc_title;
    public int Release_Year { get; } = release_year;
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
