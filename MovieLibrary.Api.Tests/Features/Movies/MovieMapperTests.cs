using MovieLibrary.Api.Features.Common;
using MovieLibrary.Api.Features.Movies;
using MovieLibrary.Api.Infrastructure.Tmdb;

namespace MovieLibrary.Api.Tests.Features.Movies;


public class MovieMapperTests
{
    private static TmdbMovie ValidMovie() => new()
    {
        Id = 1,
        Title = "Test Movie",
        Overview = "This is a test movie.",
        PosterPath = "/testposter.jpg",
        BackdropPath = "/testbackdrop.jpg",
        ReleaseDate = "2023-01-01",
        VoteAverage = 8.5,
        OriginalLanguage = "en"

    };
    private static MovieItem ValidMovieItem() => new()
    {
        Id = 1,
        Title = "Test Movie",
        Overview = "This is a test movie.",
        PosterUrl = "https://image.tmdb.org/t/p/w500/testposter.jpg",
        BackdropUrl = "https://image.tmdb.org/t/p/w1280/testbackdrop.jpg",
        ReleaseDate = new DateOnly(2023, 1, 1),
        VoteAverage = 8.5,
        OriginalLanguage = "en"
    };

    [Fact]
    public void ToMovieItem_ValidMovie_MapsAllFields()
    {
        // Arrange
        var tmdbMovie = ValidMovie();
        var expected = ValidMovieItem();

        // Act
        var movieItem = tmdbMovie.ToMovieItem();
            

        // Assert
        Assert.Equal(expected, movieItem);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ToMovieItem_MissingPosterPath_ReturnsNullPosterUrl(string? posterPath)
    {
        // Arrange
        var tmdbMovie = ValidMovie() with { PosterPath = posterPath };
        
        // Act
        var movieItem = tmdbMovie.ToMovieItem();
        // Assert
        Assert.Null(movieItem.PosterUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ToMovieItem_MissingBackdropPath_ReturnsNullBackdropUrl(string? backdropPath)
    {
        // Arrange
        var tmdbMovie = ValidMovie() with { BackdropPath = backdropPath };

        // Act
        var movieItem = tmdbMovie.ToMovieItem();
        // Assert
        Assert.Null(movieItem.BackdropUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("invalid-date")]
    [InlineData("2023-13-01")] // Invalid month
    [InlineData("2023-02-30")] // Invalid day
    [InlineData("2023/01/01")] // Wrong format
    public void ToMovieItem_InvalidReleaseDate_ReturnsNullReleaseDate(string? releaseDate)
    {
        // Arrange
        var tmdbMovie = ValidMovie() with { ReleaseDate = releaseDate };
        // Act
        var movieItem = tmdbMovie.ToMovieItem();
        // Assert
        Assert.Null(movieItem.ReleaseDate);
    }
    
    [Fact]
    public void ToMovieItem_ValidReleaseDate_ReturnsCorrectReleaseDate()
    {
        // Arrange
        var tmdbMovie = ValidMovie() with { ReleaseDate = "2023-12-31" };
        var expectedDate = new DateOnly(2023, 12, 31);
        // Act
        var movieItem = tmdbMovie.ToMovieItem();
        // Assert
        Assert.Equal(expectedDate, movieItem.ReleaseDate);
    }
    
    [Fact]
    public void ToPagedResponse_ValidPagedResponse_MapsAllFields()
    {
        // Arrange
        var tmdbPagedResponse = new TmdbPagedResponse
        {
            Page = 1,
            TotalResults = 100,
            TotalPages = 10,
            Results = [ValidMovie(), ValidMovie() with { Id = 2, Title = "Test Movie 2" }]
        };
        var expected = new PagedResponse<MovieItem>
        {
            Page = 1,
            TotalResults = 100,
            TotalPages = 10,
            Results = [ValidMovieItem(), ValidMovieItem() with { Id = 2, Title = "Test Movie 2" }]
        };
        // Act
        var pagedResponse = tmdbPagedResponse.ToPagedResponse();
        // Assert
        Assert.Equal(expected.Page, pagedResponse.Page);
        Assert.Equal(expected.TotalResults, pagedResponse.TotalResults);
        Assert.Equal(expected.TotalPages, pagedResponse.TotalPages);
        Assert.Equal(expected.Results, pagedResponse.Results);
    }

    [Fact]
    public void ToPagedResponse_EmptyResults_ReturnsEmptyList()
    {
        // Arrange
        var tmdbPagedResponse = new TmdbPagedResponse
        {
            Page = 1,
            TotalResults = 0,
            TotalPages = 0,
            Results = []
        };
        var PagedResponse = tmdbPagedResponse.ToPagedResponse();

        Assert.Empty(PagedResponse.Results);
    }
}


