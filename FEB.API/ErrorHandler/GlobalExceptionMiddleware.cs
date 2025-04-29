namespace FEB.API.ErrorHandler
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                var errorResponse = new ErrorResponse
                {
                    Message = _env.IsDevelopment() ? ex.Message : "An unexpected error occurred.",
                    StackTrace = _env.IsDevelopment() ? ex.StackTrace : null
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
        public class ErrorResponse
        {
            public string Message { get; set; } = string.Empty;
            public string? StackTrace { get; set; }
        }
    }

}
