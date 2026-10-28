using MovieLibrary.Api.Features.Common;
using MovieLibrary.Api.Infrastructure.Tmdb;

namespace MovieLibrary.Api.Features.Movies.SearchMovies;


public class SearchMoviesHandler(ITmdbClient tmdb)
{
    public async Task<PagedResponse<MovieItem>> HandleAsync(string query, int page, CancellationToken ct)
    {

        var tmdbResponse = await tmdb.SearchMoviesAsync(query, page, ct);
        return tmdbResponse.ToPagedResponse();
    }
}
