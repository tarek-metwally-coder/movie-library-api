using MovieLibrary.Api.Features.Common;
using MovieLibrary.Api.Features.Movies;
using MovieLibrary.Api.Tests.Fixtures;
using System.Net;
using System.Net.Http.Json;

namespace MovieLibrary.Api.Tests.Features.Movies.SearchMovies;

public class SearchMoviesEndpointTests : IClassFixture<MovieLibraryApiFactory>
{
    private readonly MovieLibraryApiFactory _factory;

    public SearchMoviesEndpointTests(MovieLibraryApiFactory factory)
    {
        _factory = factory;
        _factory.Stub.Reset();

    }

    [Fact]
    public async Task SearchMovies_ValidTmdbResponse_ReturnsOkWithMappedResults()
    {
        // arrange
        _factory.Stub.StatusCodeToReturn = HttpStatusCode.OK;
        _factory.Stub.JsonBodyToReturn =
            // lang=json
            """
            {
            "page":3,
            "results":
            [
            {
              "adult": false,
              "backdrop_path": "/8YFL5QQVPy3AgrEQxNYVSgiPEbe.jpg",
              "genre_ids": [
                28,
                12,
                878
              ],
              "id": 640146,
              "original_language": "en",
              "original_title": "Ant-Man and the Wasp: Quantumania",
              "overview": "Super-Hero partners Scott Lang and Hope van Dyne, along with with Hope's parents Janet van Dyne and Hank Pym, and Scott's daughter Cassie Lang, find themselves exploring the Quantum Realm, interacting with strange new creatures and embarking on an adventure that will push them beyond the limits of what they thought possible.",
              "popularity": 9272.643,
              "poster_path": "/ngl2FKBlU4fhbdsrtdom9LVLBXw.jpg",
              "release_date": "2023-02-15",
              "title": "Ant-Man and the Wasp: Quantumania",
              "video": false,
              "vote_average": 6.5,
              "vote_count": 1856
            }
            ],
            "total_pages":7,
            "total_results":140
            }
            """;


        // act
        var client = _factory.CreateClient();
        using var response = await client.GetAsync("/api/movies/search?query=Ant&page=3", TestContext.Current.CancellationToken);
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<MovieItem>>(TestContext.Current.CancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(7, body.TotalPages);
        Assert.Equal(140, body.TotalResults);
        var movie = Assert.Single(body.Results);
        Assert.Equal(640146, movie.Id);
        Assert.Equal(new DateOnly(2023, 2, 15), movie.ReleaseDate);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError, "Internal Server Error")]
    [InlineData(HttpStatusCode.OK, """{ "page": 3, "results": null, "total_pages": 7, "total_results": 140 } """)]
    [InlineData(HttpStatusCode.OK, "{not json}")]
    public async Task SearchMovies_UpstreamFailure_Returns502(HttpStatusCode tmdbStatus, string tmdbBody)
    {
        // arrange
        _factory.Stub.StatusCodeToReturn = tmdbStatus;
        _factory.Stub.JsonBodyToReturn = tmdbBody;


        var client = _factory.CreateClient();

        // act
        using var response = await client.GetAsync("/api/movies/search?query=Ant&page=3", TestContext.Current.CancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/movies/search")]
    [InlineData("/api/movies/search?query=Ant&page=0")]
    [InlineData("/api/movies/search?query=Ant&page=501")]
    [InlineData("/api/movies/search?query=&page=3")]
    [InlineData("/api/movies/search?query=abc&page=abc")]
    public async Task SearchMovies_InvalidParameters_Returns400(string url)
    {
        // arrange
        var client = _factory.CreateClient();
        // act
        using var response = await client.GetAsync(url, TestContext.Current.CancellationToken);
        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Null(_factory.Stub.ReceivedRequest);
    }

}
