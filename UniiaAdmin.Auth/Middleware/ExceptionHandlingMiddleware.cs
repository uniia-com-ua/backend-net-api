public class ExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionHandlingMiddleware> _logger;

	public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unexpected error occurred");
			await HandleExceptionAsync(context, ex);
		}
	}

	private static Task HandleExceptionAsync(HttpContext context, Exception ex)
	{
		context.Response.ContentType = "application/json";
		context.Response.StatusCode = StatusCodes.Status400BadRequest;

		var result = System.Text.Json.JsonSerializer.Serialize(new
		{
			error = ex.Message
		});

		return context.Response.WriteAsync(result);
	}
}
