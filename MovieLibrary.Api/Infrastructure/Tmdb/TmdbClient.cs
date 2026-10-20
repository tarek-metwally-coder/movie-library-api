namespace MovieLibrary.Api.Infrastructure.Tmdb;

public class TmdbClient(HttpClient httpClient)
{
    public async Task<TmdbPagedResponse> SearchMoviesAsync(string query, int page, CancellationToken ct)
    {
        string escapedQuery = Uri.EscapeDataString(query);
        var response = await httpClient.GetFromJsonAsync<TmdbPagedResponse>($"search/movie?query={escapedQuery}&page={page}", ct);
        return response ?? new TmdbPagedResponse();
    }
    public async Task<TmdbPagedResponse> GetPopularMoviesAsync(int page, CancellationToken ct)
    {
        var response = await httpClient.GetFromJsonAsync<TmdbPagedResponse>($"movie/popular?page={page}", ct);
        return response ?? new TmdbPagedResponse();
    }
}