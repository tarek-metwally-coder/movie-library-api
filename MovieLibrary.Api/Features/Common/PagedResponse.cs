namespace MovieLibrary.Api.Features.Common;

public record PagedResponse<T>
{
    public int Page { get; init; }
    public int TotalResults { get; init; }
    public int TotalPages { get; init; }
    public List<T> Results { get; init; } = [];
}