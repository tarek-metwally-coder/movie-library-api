namespace MovieLibrary.Api.Features.Movies;

public record MovieItem
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string? Overview { get; init; }
    public string? PosterUrl { get; init; }
    public string? BackdropUrl { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public double VoteAverage { get; init; }
    public string? OriginalLanguage { get; init; }
}