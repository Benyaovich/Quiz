using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Quiz.DataAccess.Exceptions;

namespace Quiz.API.Infrastructure
{
    public class ExceptionToProblemDetailsHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            return exception switch
            {
                EntityNotFoundException => await CreateProblemDetails(httpContext, exception,
                    StatusCodes.Status404NotFound),
                AccessViolationException => await CreateProblemDetails(httpContext, exception,
                    StatusCodes.Status403Forbidden),
                ArgumentOutOfRangeException => await CreateProblemDetails(httpContext, exception,
                    StatusCodes.Status400BadRequest),
                ArgumentNullException => await CreateProblemDetails(httpContext, exception,
                    StatusCodes.Status400BadRequest),
                ArgumentException => await CreateProblemDetails(httpContext, exception,
                    StatusCodes.Status409Conflict),
                InvalidDataException => await CreateProblemDetails(httpContext, exception,
                    StatusCodes.Status409Conflict),
                InvalidOperationException => await CreateProblemDetails(httpContext, exception,
                    StatusCodes.Status409Conflict),
                _ => false
            };
        }

        private async Task<bool> CreateProblemDetails(HttpContext httpContext, Exception exception, int statusCode)
        {
            httpContext.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Title = "An error occurred",
                Type = exception.GetType().Name,
                Detail = exception.Message,
            };

            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                Exception = exception,
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
        }
    }

}
