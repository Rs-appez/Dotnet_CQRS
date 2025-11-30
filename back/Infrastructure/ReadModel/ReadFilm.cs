using MongoDB.Bson;
using Application.Films;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.ReadModel;

public class ReadFilm(ObjectId id, int Film_Id, string Fr_Title, string Qc_Title, int Release_Year) 
    : FilmDto(Film_Id, Fr_Title, Qc_Title, Release_Year)
{
    [BsonId]
    public ObjectId Id { get; set; } = id;

    public override string ToString()
    {
        return $"ReadFilm {{ Id = {Id}, Film_Id = {Film_Id}, Fr_Title = {Fr_Title}, Qc_Title = {Qc_Title}, Release_Year = {Release_Year} }}";
    }
}
