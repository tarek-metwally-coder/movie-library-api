using System.ComponentModel.DataAnnotations;

namespace MovieLibrary.Api.Infrastructure.Tmdb;

public class TmdbOptions
{
    [Required]
    public string BaseUrl { get; set; } = string.Empty;
    [Required]
    public string ApiKey { get; set; } = string.Empty;
}

