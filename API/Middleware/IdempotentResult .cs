namespace API.Middleware
{
    internal sealed class IdempotentResult : IResult
    {
        private readonly int _statusCode;
        private readonly object? _value;

        public IdempotentResult (int statusCode, object? value)
        {
            _statusCode = statusCode;
            _value = value;
        }

        public Task ExecuteAsync (HttpContext httpContext)
        {
            httpContext.Response.StatusCode = _statusCode;

            return httpContext.Response.WriteAsJsonAsync (_value);
        }
    }
}
