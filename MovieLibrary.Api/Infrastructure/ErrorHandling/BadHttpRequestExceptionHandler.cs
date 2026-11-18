using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MovieLibrary.Api.Infrastructure.ErrorHandling;

public class BadHttpRequestExceptionHandler(
    ILogger<BadHttpRequestExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is BadHttpRequestException badRequest)
        {
            logger.LogWarning(exception, "Bad HTTP request encountered.");
            var problemDetails = new ProblemDetails
            {
                Title = "Bad Request",
                Detail = "The request could not be processed due to a client error.",
                Status = badRequest.StatusCode,
                Instance = context.Request.Path,
            };
            context.Response.StatusCode = badRequest.StatusCode;
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
