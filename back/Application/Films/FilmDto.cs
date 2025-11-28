namespace Application.Films;

public class FilmDto(int Film_Id, string Fr_Title, string Qc_Title, int Release_Year)
{
    public int Film_Id { get; set; } = Film_Id;
    public string Fr_Title { get; set; } = Fr_Title;
    public string Qc_Title { get; set; } = Qc_Title;
    public int Release_Year { get; set; } = Release_Year;

    public override string ToString()
    {
        return $"FilmDto {{ Film_Id = {Film_Id}, Fr_Title = {Fr_Title}, Qc_Title = {Qc_Title}, Release_Year = {Release_Year} }}";
    }
}
