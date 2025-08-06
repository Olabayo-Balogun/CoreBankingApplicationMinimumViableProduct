namespace API.Middleware
{
	/// <summary>
	/// Middleware to handle JWT token extraction from the Authorization header in HTTP requests.
	/// </summary>
	public class JwtTokenMiddleware
	{
		private readonly RequestDelegate _next;

		/// <summary>
		/// Constructor for JwtTokenMiddleware that takes a RequestDelegate as a parameter.
		/// </summary>
		/// <param name="next"></param>
		public JwtTokenMiddleware (RequestDelegate next)
		{
			_next = next;
		}

		/// <summary>
		/// Middleware to extract JWT token from the Authorization header and store it in the HttpContext.Items collection.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public async Task InvokeAsync (HttpContext context)
		{
			if (context.Request.Headers.ContainsKey ("Authorization"))
			{
				var token = context.Request.Headers["Authorization"].ToString ().Split (" ").Last ();
				context.Items["JwtToken"] = token;
			}

			await _next (context);
		}
	}
}
