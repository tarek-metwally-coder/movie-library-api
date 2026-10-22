
using System.Text.Json.Serialization;

namespace MovieLibrary.Api.Infrastructure.Tmdb;

public record TmdbMovie
{
    [JsonPropertyName("id")]
    public int Id { get; init; }
    [JsonPropertyName("title")]
    public string? Title { get; init; }
    [JsonPropertyName("overview")]
    public string? Overview { get; init; }
    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; init; }
    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; init; }
    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; init; }
    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; init; }
    [JsonPropertyName("original_language")]
    public string? OriginalLanguage { get; init; }

}