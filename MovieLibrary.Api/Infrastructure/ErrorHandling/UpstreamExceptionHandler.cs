using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MovieLibrary.Api.Infrastructure.ErrorHandling;

public class UpstreamExceptionHandler(
    ILogger<UpstreamExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler

{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is HttpRequestException or JsonException)
        {
            logger.LogError(exception, "An upstream service error occurred.");
            var problemDetails = new ProblemDetails
            {
                Title = "Upstream Service Error",
                Detail = "An error occurred while communicating with an upstream service.",
                Status = StatusCodes.Status502BadGateway,
                Instance = context.Request.Path,
            };
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problemDetails,
                Exception = exception

            });
        }
        return false;
    }

}
