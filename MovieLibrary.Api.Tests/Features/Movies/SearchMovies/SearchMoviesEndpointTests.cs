using MovieLibrary.Api.Tests.Fixtures;
using System.Net;

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
    public async Task SearchMovies_ValidRequest_ReturnsOKResponse()
    {
        // arrange
        _factory.Stub.StatusCodeToReturn = HttpStatusCode.OK;
        _factory.Stub.JsonBodyToReturn =
            // lang=json
            """
            {
            "page":1,
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
            "total_pages":1,
            "total_results":1
            }
            """;


        // act
        var client = _factory.CreateClient();
        using var response = await client.GetAsync($"/api/movies/search?query=Ant&page=1", TestContext.Current.CancellationToken);


        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

}
