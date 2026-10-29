using MovieLibrary.Api.Features.Movies;
using MovieLibrary.Api.Features.Movies.SearchMovies;
using MovieLibrary.Api.Infrastructure.Tmdb;
using MovieLibrary.Api.Tests.TestDoubles;
using System.Text.Json;

namespace MovieLibrary.Api.Tests.Features.Movies.SearchMovies;

public class SearchMoviesHandlerTests
{
    public static TheoryData<Exception> UpstreamExceptions =>
    [
        new HttpRequestException("network down"),
        new JsonException("bad json"),
    ];

    [Fact]
    public async Task HandleAsync_ValidInputs_ForwardsQueryPageAndTokenToClient()
    {
        // arrange
        string query = "test query";
        int page = 3;
        using var cts = new CancellationTokenSource();

        var stubTmdbClient = new StubTmdbClient();
        var handler = new SearchMoviesHandler(stubTmdbClient);

        // act
        await handler.HandleAsync(query, page, cts.Token);

        // assert

        Assert.Equal(query, stubTmdbClient.ReceivedQuery);
        Assert.Equal(page, stubTmdbClient.ReceivedPage);
        Assert.Equal(cts.Token, stubTmdbClient.ReceivedCt);


    }

    [Fact]
    public async Task HandleAsync_TmdbReturnsResponse_ResponseMappedCorrectly()
    {
        // arrange
        var tmdbResponse = new TmdbPagedResponse
        {
            Page = 5,
            TotalPages = 1,
            TotalResults = 2,
            Results = [
                new TmdbMovie{
                    Id = 1,
                    Title = "test1"
                },
                new TmdbMovie{
                    Id = 2,
                    Title = "test2"
                }
                ]
        };

        var expected = tmdbResponse.ToPagedResponse();

        var stub = new StubTmdbClient
        {
            ResponseToReturn = tmdbResponse
        };
        var handler = new SearchMoviesHandler(stub);

        // act
        var result = await handler.HandleAsync("test", 3, CancellationToken.None);

        // assert

        Assert.Equivalent(expected, result);

    }

    [Theory]
    [MemberData(nameof(UpstreamExceptions), DisableDiscoveryEnumeration = true)]
    public async Task HandleAsync_ClientThrowsUpstreamException_PropagatesSameException(Exception ex)
    {
        // arrange 
        var stub = new StubTmdbClient
        {
            ExceptionToThrow = ex
        };

        var handler = new SearchMoviesHandler(stub);

        // act + assert
        var caught = await Assert.ThrowsAnyAsync<Exception>(
            () => handler.HandleAsync("test", 3, CancellationToken.None));

        Assert.Same(ex, caught);

    }


}
