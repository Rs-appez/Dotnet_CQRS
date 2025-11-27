using MongoDB.Bson;
using Domain.Entities;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Read.Models;

public class ReadFilm(ObjectId id, int film_id, string fr_title, string qc_title, int release_year) 
    : Film(film_id, fr_title, qc_title, release_year)
{
    [BsonId]
    public ObjectId Id { get; set; } = id;
}
