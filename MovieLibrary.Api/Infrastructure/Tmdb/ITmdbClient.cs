namespace MovieLibrary.Api.Infrastructure.Tmdb;

public interface ITmdbClient
{
    Task<TmdbPagedResponse> GetPopularMoviesAsync(int page, CancellationToken ct);
    Task<TmdbPagedResponse> SearchMoviesAsync(string query, int page, CancellationToken ct);
}