using System.ComponentModel.DataAnnotations;

namespace MovieLibrary.Api.Features.Movies.SearchMovies;

public static class SearchMoviesEndpoint
{
    public static IEndpointRouteBuilder MapSearchMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/movies/search", async ([Required] string query, SearchMoviesHandler handler, CancellationToken ct, [Range(1, 500)] int page = 1) =>
        {
            var response = await handler.HandleAsync(query, page, ct);
            return TypedResults.Ok(response);
        })
        .ProducesValidationProblem();

        return app;
    }
}
