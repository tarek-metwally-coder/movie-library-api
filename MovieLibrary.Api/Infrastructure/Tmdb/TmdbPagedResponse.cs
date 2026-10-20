using System.Text.Json.Serialization;

namespace MovieLibrary.Api.Infrastructure.Tmdb;


public  record TmdbPagedResponse
{
    [JsonPropertyName("page")]
    public int Page { get; init; }
    [JsonPropertyName("total_results")]
    public int TotalResults { get; init; }
    [JsonPropertyName("total_pages")]
    public int TotalPages { get; init; }
    [JsonPropertyName("results")]
    public List<TmdbMovie> Results { get; init; } = [];

}