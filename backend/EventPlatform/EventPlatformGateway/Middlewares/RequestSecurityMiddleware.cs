namespace EventPlatformGateway.Middlewares
{
    public sealed class RequestSecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestSecurityMiddleware> _logger;

        public RequestSecurityMiddleware(RequestDelegate next, ILogger<RequestSecurityMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Zabrana sumnjivih putanja.
            if (context.Request.Path.Value?.Contains("..", StringComparison.Ordinal) == true)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid request path.");
                return;
            }

            // Za sve mutirajuće zahteve tražimo dodatni header.
            if ((HttpMethods.IsPost(context.Request.Method) ||
                 HttpMethods.IsPut(context.Request.Method) ||
                 HttpMethods.IsDelete(context.Request.Method) ||
                 HttpMethods.IsPatch(context.Request.Method)) &&
                !context.Request.Headers.ContainsKey("X-Request-Source"))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Missing X-Request-Source header.");
                return;
            }

            // Gruba zaštita od prevelikih body-ja.
            if (context.Request.ContentLength is > 5_000_000)
            {
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                await context.Response.WriteAsync("Request body is too large.");
                return;
            }

            _logger.LogInformation("Request passed security checks: {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await _next(context);
        }
    }
}
}
