using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace MovieLibrary.Api.Tests.TestDoubles;

public class StubHttpMessageHandler : HttpMessageHandler
{
    public HttpStatusCode StatusCodeToReturn { get; set; }
    public string JsonBodyToReturn { get; set; } 
        
    public HttpRequestMessage? ReceivedRequest { get; private set; }

    public StubHttpMessageHandler()
    {
        Reset();

    }
    
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ReceivedRequest = request;

        var response = new HttpResponseMessage(StatusCodeToReturn)
        {
            Content = new StringContent(JsonBodyToReturn, Encoding.UTF8, "application/json")
        };

        return Task.FromResult(response);

    }

    [MemberNotNull(nameof(JsonBodyToReturn))]
    public void Reset()
    {
        StatusCodeToReturn = HttpStatusCode.OK;
        JsonBodyToReturn =
        // lang=json
        """{"page":1,"results":[],"total_pages":0,"total_results":0}""";
        ReceivedRequest = null;
    }
}
