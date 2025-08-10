using Application.Model;
using Application.Models.Users.Command;
using Application.Models.Users.Queries;
using Application.Models.Users.Response;
using Application.Utility;

using Asp.Versioning;

using Domain.Enums;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
	[ApiVersion ("1.0")]
	[Route ("api/v{version:apiVersion}/[controller]")]
	[ApiController]
	[Authorize]
	[ProducesResponseType (StatusCodes.Status204NoContent)]
	[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status400BadRequest)]
	[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status401Unauthorized)]
	[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status404NotFound)]
	[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status429TooManyRequests)]
	[ProducesResponseType (StatusCodes.Status500InternalServerError)]
	public class UsersController : ControllerBase
	{
		private readonly IMediator _mediator;

		public UsersController (IMediator mediator)
		{
			_mediator = mediator;
		}

		/// <summary>
		/// Lets admin and staff see a users' information
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet ("user/{id}")]
		[Authorize (Roles = $"{UserRoles.Admin}, {UserRoles.Staff}")]
		[ResponseCache (Duration = 3600, VaryByQueryKeys = new[] { "id" })]
		[EnableRateLimiting ("GetRequestRateLimit")]
		[ProducesResponseType (type: typeof (UserResponse), StatusCodes.Status200OK)]
		public async Task<ActionResult<UserResponse>> GetUserById ([FromRoute] string id, CancellationToken cancellationToken)
		{
			var request = new UserQuery
			{
				UserPublicId = id,
				IsCount = false,
				CancellationToken = cancellationToken
			};
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}


		/// <summary>
		/// Lets admin get statistics of users on the platform
		/// </summary>
		/// <param name="role">This returns the count of users that have a specific user role on the platform</param>
		/// <param name="date">This returns a count of users according to date created, deleted, or active within a particular period</param>
		/// <param name="isDeleted">If set to true it returns a count of users who have been deleted</param>
		/// <param name="period">This is used with date to get active users within a period (daily, weekly, monthly, or yearly) of time</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet ("count")]
		[Authorize (Roles = UserRoles.Admin)]
		[ResponseCache (Duration = 3600, VaryByQueryKeys = new[] { "isDeleted", "date", "role", "period" })]
		[EnableRateLimiting ("GetRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<UserResponse>>> GetCountOfUsers ([FromQuery] DateTime? date, string? period, string? role, bool? isDeleted, CancellationToken cancellationToken)
		{
			if (date != null)
			{
				if (date.Value > DateTime.UtcNow)
				{
					return BadRequest ("Date cannot be in the future");
				}
			}

			var request = new UserQuery
			{
				Role = role,
				IsCount = true,
				Date = date,
				Period = period,
				IsDeleted = isDeleted,
				CancellationToken = cancellationToken
			};
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		///  Lets admin and staff spool information on users
		/// </summary>
		/// <param name="role"></param>
		/// <param name="isDeleted">This is used to get created or deleted users and it can be paired with date to get users that were created or deleted on a specific date</param>
		/// <param name="date">This is used to get users that were created or deleted on a specific date</param>
		/// <param name="country">This is used to get users by their country of origin or residence</param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpGet ("users")]
		[Authorize (Roles = $"{UserRoles.Admin}, {UserRoles.Staff}")]
		[ResponseCache (Duration = 300, VaryByQueryKeys = new[] { "isDeleted", "date", "role", "country", "pageNumber", "pageSize" })]
		[EnableRateLimiting ("GetRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<List<UserResponse>>), StatusCodes.Status200OK)]
		[ProducesResponseType (type: typeof (RequestResponse<List<UserResponse>>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType (type: typeof (RequestResponse<List<UserResponse>>), StatusCodes.Status401Unauthorized)]
		[ProducesResponseType (type: typeof (RequestResponse<List<UserResponse>>), StatusCodes.Status404NotFound)]
		[ProducesResponseType (type: typeof (RequestResponse<List<UserResponse>>), StatusCodes.Status429TooManyRequests)]
		public async Task<ActionResult<RequestResponse<List<UserResponse>>>> GetAllUser ([FromQuery] DateTime? date, string? role, string? country, bool? isDeleted, CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 10)
		{
			if (date != null)
			{
				if (date.Value > DateTime.UtcNow)
				{
					return BadRequest ("Date cannot be in the future");
				}
			}

			var request = new UsersQuery
			{
				Role = role,
				Date = date,
				Country = country,
				PageNumber = pageNumber,
				PageSize = pageSize,
				IsDeleted = isDeleted,
				CancellationToken = cancellationToken
			};
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}


		/// <summary>
		/// Lets admin update a users' details
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPut ("user")]
		[Authorize (Roles = UserRoles.Admin)]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (UserResponse), StatusCodes.Status200OK)]
		public async Task<ActionResult<UserResponse>> UpdateUser ([FromBody] UpdateUserProfileCommand request, CancellationToken cancellationToken)
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

		/// <summary>
		/// Lets admin change a users' role.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPut ("role")]
		[Authorize (Roles = UserRoles.Admin)]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (UserResponse), StatusCodes.Status200OK)]
		public async Task<ActionResult<UserResponse>> UpdateUserRole ([FromBody] UpdateUserRoleCommand request, CancellationToken cancellationToken)
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
			request.UserId = tokenResponse.UserId;
			request.CancellationToken = cancellationToken;
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}


		/// <summary>
		/// Lets a user update his/her profile picture
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpPut ("profile-image")]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (UserResponse), StatusCodes.Status200OK)]
		public async Task<ActionResult<UserResponse>> UpdateUserProfileImage ([FromBody] UpdateUserProfileImageCommand request, CancellationToken cancellationToken)
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

		/// <summary>
		/// Lets admin delete a user's account
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpDelete ("user")]
		[Authorize (Roles = UserRoles.Admin)]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<UserResponse>>> DeleteUser ([FromBody] DeleteUserCommand request, CancellationToken cancellationToken)
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
			request.DeletedBy = tokenResponse.UserId;
			request.CancellationToken = cancellationToken;
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}

		/// <summary>
		/// Lets admin mark multiple users as deleted.
		/// </summary>
		/// <param name="request"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		[HttpDelete ("users")]
		[Authorize (Roles = UserRoles.Admin)]
		[EnableRateLimiting ("PostRequestRateLimit")]
		[ProducesResponseType (type: typeof (RequestResponse<UserResponse>), StatusCodes.Status200OK)]
		public async Task<ActionResult<RequestResponse<UserResponse>>> DeleteMultipleUser ([FromBody] DeleteUsersCommand request, CancellationToken cancellationToken)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest (request);
			}

			var payloadValidation = Utility.IsExceedingPostPayloadLimit (request.UserIds.Count);

			if (!payloadValidation.IsValid)
			{
				return BadRequest (payloadValidation.Remark);
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
			request.DeletedBy = tokenResponse.UserId;
			request.CancellationToken = cancellationToken;
			var result = await _mediator.Send (request);
			return StatusCode (result.StatusCode, result);
		}
	}
}
