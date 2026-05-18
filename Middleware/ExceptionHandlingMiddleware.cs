using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace school_diary.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _log;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> log)
        {
            _next = next;
            _log  = log;
        }

        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unhandled exception");
                await WriteProblemDetailsAsync(ctx, ex);
            }
        }

        private static Task WriteProblemDetailsAsync(HttpContext ctx, Exception ex)
        {
            var status = ex switch
            {
                KeyNotFoundException       => HttpStatusCode.NotFound,
                InvalidOperationException   => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Forbidden,
                _                           => HttpStatusCode.InternalServerError
            };

            var problem = new
            {
                type   = $"https://httpstatuses.com/{(int)status}",
                title  = status.ToString(),
                status = (int)status,
                detail = ex.Message,
                traceId = ctx.TraceIdentifier
            };

            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode  = (int)status;

            var json = JsonSerializer.Serialize(problem);
            return ctx.Response.WriteAsync(json);
        }
    }
}