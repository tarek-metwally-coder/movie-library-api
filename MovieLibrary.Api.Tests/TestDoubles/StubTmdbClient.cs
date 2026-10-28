
using MovieLibrary.Api.Infrastructure.Tmdb;

namespace MovieLibrary.Api.Tests.TestDoubles;

public class StubTmdbClient : ITmdbClient
{
    
    public Exception? ExceptionToThrow { get; set; }
    public TmdbPagedResponse ResponseToReturn { get; set; } = new TmdbPagedResponse();
    public string ReceivedQuery { get; private set; } = string.Empty;
    public int ReceivedPage { get; private set; } = 0;
    public CancellationToken ReceivedCt { get; private set; } = CancellationToken.None;

    public Task<TmdbPagedResponse> GetPopularMoviesAsync(int page, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<TmdbPagedResponse> SearchMoviesAsync(string query, int page, CancellationToken ct)
    {
        ReceivedQuery = query;
        ReceivedPage = page;
        ReceivedCt = ct;

       if (ExceptionToThrow != null)
        {
            throw ExceptionToThrow;
        }
        return Task.FromResult(ResponseToReturn);
    }
}
