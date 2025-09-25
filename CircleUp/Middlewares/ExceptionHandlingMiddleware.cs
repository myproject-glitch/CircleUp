using System.Net;
using Serilog;

namespace CircleUp.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);

                // Handle non-success status codes AFTER pipeline
                if (context.Response.StatusCode == (int)HttpStatusCode.NotFound ||
                    context.Response.StatusCode == (int)HttpStatusCode.Unauthorized ||
                    context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
                {
                    await HandleStatusCodeAsync(context, (HttpStatusCode)context.Response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var request = context.Request;

            Log.ForContext("RequestId", context.TraceIdentifier)
               .ForContext("Path", request.Path)
               .ForContext("ClientIP", context.Connection.RemoteIpAddress?.ToString())
               .ForContext("UserAgent", request.Headers["User-Agent"].ToString())
               .Error(exception, "Unhandled exception");

            context.Response.Clear();
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.Redirect("/Home/Error?code=500");

            await Task.CompletedTask;
        }

        private async Task HandleStatusCodeAsync(HttpContext context, HttpStatusCode statusCode)
        {
            var request = context.Request;

            Log.ForContext("RequestId", context.TraceIdentifier)
               .ForContext("Path", request.Path)
               .ForContext("ClientIP", context.Connection.RemoteIpAddress?.ToString())
               .ForContext("UserAgent", request.Headers["User-Agent"].ToString())
               .Warning("Handled status code {StatusCode}", statusCode);

            context.Response.Clear();
            context.Response.StatusCode = (int)statusCode;
            context.Response.Redirect($"/Home/Error?code={(int)statusCode}");

            await Task.CompletedTask;
        }
    }
}
