using MovieLibrary.Api.Features.Common;
using MovieLibrary.Api.Infrastructure.Tmdb;
using System.Globalization;

namespace MovieLibrary.Api.Features.Movies;

public static class MovieMapper
{
    private const string BaseImageUrl = "https://image.tmdb.org/t/p/w500";
    private const string BaseBackdropUrl = "https://image.tmdb.org/t/p/w1280";


    public static MovieItem ToMovieItem(this TmdbMovie tmdbMovie)
    {
        return new MovieItem
        {
            Id = tmdbMovie.Id,
            Title = tmdbMovie.Title,
            Overview = tmdbMovie.Overview,
            PosterUrl = string.IsNullOrWhiteSpace(tmdbMovie.PosterPath) ? null : $"{BaseImageUrl}{tmdbMovie.PosterPath}",
            BackdropUrl = string.IsNullOrWhiteSpace(tmdbMovie.BackdropPath) ? null : $"{BaseBackdropUrl}{tmdbMovie.BackdropPath}",
            ReleaseDate = ParseReleaseDate(tmdbMovie.ReleaseDate),
            VoteAverage = tmdbMovie.VoteAverage,
            OriginalLanguage = tmdbMovie.OriginalLanguage
        };
    }
    public static PagedResponse<MovieItem> ToPagedResponse(this TmdbPagedResponse tmdbPagedResponse)
    {
        return new PagedResponse<MovieItem>
        {
            Page = tmdbPagedResponse.Page,
            TotalResults = tmdbPagedResponse.TotalResults,
            TotalPages = tmdbPagedResponse.TotalPages,
            Results = [.. tmdbPagedResponse.Results.Select(r => r.ToMovieItem())]
        };
    }
    private static DateOnly? ParseReleaseDate(string? releaseDate)
    {
        if (DateOnly.TryParseExact(releaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }
        return null;
    }
}