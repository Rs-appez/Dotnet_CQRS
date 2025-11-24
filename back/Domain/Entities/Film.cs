namespace Domain.Entities;

public class Film(int id, string fr_title, string qc_title, int release_year)
{
    public int Id { get; set; } = id;
    public string Fr_Title { get; set; } = fr_title;
    public string Qc_Title { get; set; } = qc_title;
    public int Release_Year { get; set; } = release_year;

    public Film(string fr_title, string qc_title, int release_year)
        : this(0, fr_title, qc_title, release_year) { }

}
