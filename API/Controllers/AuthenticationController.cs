using API.Middleware;

using Application.Models;
using Application.Models.Users.Command;
using Application.Models.Users.Response;
using Application.Utility;

using Asp.Versioning;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
	[ApiVersion ("1.0")]
	[Route ("api/v{version:apiVersion}/[controller]")]
	[ApiController]
	[ProducesResponseType (StatusCodes.Status204NoContent)]
	[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status401Unauthorized)]
	[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status404NotFound)]
	[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status429TooManyRequests)]
	[ProducesResponseType (StatusCodes.Status500InternalServerError)]
	public class AuthenticationController : ControllerBase
	{
		private readonly IMediator _mediator;
		public AuthenticationController (IMediator mediator)
		{
			_mediator = mediator;
		}

		/// <summary>
		/// Lets any registered user with verified email address to login
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost ("login")]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[AllowAnonymous]
		[ProducesResponseType (type: typeof (RequestResponse<LoginResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType (type: typeof (RequestResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType (type: typeof (RequestResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType (type: typeof (RequestResponse<LoginResponse>), StatusCodes.Status404NotFound)]
		[ProducesResponseType (type: typeof (RequestResponse<LoginResponse>), StatusCodes.Status429TooManyRequests)]
		public async Task<ActionResult<RequestResponse<LoginResponse>>> Login ([FromBody] LoginCommand request, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest (request);
			}

			request.CancellationToken = cancellationToken;
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// Lets any logged in user to logout
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost ("logout")]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[Authorize]
		[ProducesResponseType (type: typeof (RequestResponse<LogoutResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType (type: typeof (RequestResponse<LogoutResponse>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType (type: typeof (RequestResponse<LogoutResponse>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType (type: typeof (RequestResponse<LogoutResponse>), StatusCodes.Status404NotFound)]
		[ProducesResponseType (type: typeof (RequestResponse<LogoutResponse>), StatusCodes.Status429TooManyRequests)]
		public async Task<ActionResult<RequestResponse<LogoutResponse>>> Logout ([FromBody] CancellationToken cancellationToken)
		{
			var token = HttpContext.Items["JwtToken"] as string;
			var tokenResponse = Utility.ValidateToken (token);

			if (token == null)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (!tokenResponse.IsValid)
			{
				return BadRequest (tokenResponse.Remark);
			}

			LogoutCommand request = new ()
			{
				Token = token,
				CancellationToken = cancellationToken
			};
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}


		/// <summary>
		/// Lets anybody register as a user, a unique GUID must be passed as the value (for each unique request) to a header key "Idempotence-Key" in the header of this request
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost ("register")]
		[Idempotent (cacheTimeInSeconds: 60)]
		[AllowAnonymous]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status201Created)]
		public async Task<ActionResult<RequestResponse<UserResponse>>> Register ([FromBody] RegistrationCommand request, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest (request);
			}
			request.CancellationToken = cancellationToken;
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// Lets any registered user to verify their email on the platform.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost ("verify-email")]
		[AllowAnonymous]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<UserResponse>>> VerifyEmail ([FromBody] EmailVerificationCommand request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest (request);
			}

			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// Lets any user that forgets their password to reset their password, this sends an email to the user.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost ("forgot-password")]
		[AllowAnonymous]
		[EnableRateLimiting ("StrictPostRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<UserResponse>>> ForgotPassword ([FromQuery] ForgetPasswordCommand request, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest (request);
			}
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// Lets any registered user to request for a resend of their email vertification token
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPost ("resend-email-verification-token")]
		[AllowAnonymous]
		[EnableRateLimiting ("StrictPostRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<UserResponse>>> ResendEmailVerificationToken (ResendEmailVerificationTokenCommand request, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest (request);
			}
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// Lets any user that reset their password to change their password.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPut ("change-password")]
		[AllowAnonymous]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<UserResponse>>> ChangePassword ([FromBody] ChangePasswordCommand request, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest (request);
			}

			request.CancellationToken = cancellationToken;
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// Lets any (logged in) user change their password.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPut ("update-password")]
		[Authorize]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<UserResponse>>> UpdatePassword ([FromBody] UpdatePasswordCommand request, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest (request);
			}

			var token = HttpContext.Items["JwtToken"] as string;
			var tokenResponse = Utility.ValidateToken (token);

			if (!tokenResponse.IsValid)
			{
				return BadRequest (tokenResponse.Remark);
			}

			if (tokenResponse.UserId == null)
			{
				return BadRequest (tokenResponse.Remark);
			}

			request.LastModifiedBy = tokenResponse.UserId;
			request.CancellationToken = cancellationToken;
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}
	}
}
