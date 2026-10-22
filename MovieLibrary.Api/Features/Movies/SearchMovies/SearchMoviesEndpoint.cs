namespace MovieLibrary.Api.Features.Movies.SearchMovies;

public static class SearchMoviesEndpoint
{
    public static IEndpointRouteBuilder MapSearchMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/movies/search", async (string query, SearchMoviesHandler handler, CancellationToken ct, int page = 1) =>
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Results.BadRequest("Query parameter is required.");
            }
            if (page < 1)
            {
                return Results.BadRequest("Page parameter must be greater than 0.");
            }
            var response = await handler.HandleAsync(query, page, ct);
            return Results.Ok(response);
        });

        return app;
    }
}
