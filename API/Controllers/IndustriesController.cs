using API.Middleware;

using Application.Models;
using Application.Models.Industry.Command;
using Application.Models.Industry.Queries;
using Application.Models.Industry.Response;
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
    [ProducesResponseType (type: typeof (RequestResponse<IndustryResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType (type: typeof (RequestResponse<IndustryResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType (type: typeof (RequestResponse<IndustryResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType (type: typeof (RequestResponse<IndustryResponse>), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType (StatusCodes.Status500InternalServerError)]
    public class IndustriesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public IndustriesController (IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// An endpoint to enable a admin create an industry
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost ("industry")]
        [Authorize (Roles = UserRoles.Admin)]
        [EnableRateLimiting ("PostRequestRateLimit")]
        [Idempotent (cacheTimeInSeconds: 60)]
        [ProducesResponseType (type: typeof (RequestResponse<IndustryResponse>), StatusCodes.Status201Created)]
        public async Task<ActionResult<RequestResponse<IndustryResponse>>> CreateIndustry ([FromBody] CreateIndustryCommand request, CancellationToken cancellationToken)
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

            request.CreatedBy = tokenResponse.UserId;
            request.CancellationToken = cancellationToken;
            var result = await _mediator.Send (request);
            return StatusCode (result.StatusCode, result);
        }

        /// <summary>
        /// An endpoint to enabled logged in users get information about an industry
        /// </summary>
        /// <param name="userPublicId">This allows admin and staff to get the number of industries created by a specific user</param>
        /// <param name="id">This allows users to search for an industry using its unique ID on the database</param>
        /// <param name="name">This allows admin and staff to search for an industry using its name</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet ("industry")]
        [EnableRateLimiting ("GetRequestRateLimit")]
        [ProducesResponseType (type: typeof (RequestResponse<IndustryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<RequestResponse<IndustryResponse>>> GetIndustry ([FromQuery] long? id, string? name, string? userPublicId, CancellationToken cancellationToken)
        {
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

            if (!tokenResponse.UserId.Equals (UserRoles.Admin) && !tokenResponse.UserId.Equals (UserRoles.Staff) && userPublicId != null && !tokenResponse.UserId.Equals (userPublicId, StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized ("You are unauthorized to perform this request");
            }

            IndustryQuery request = new ()
            {
                UserId = userPublicId,
                Id = id,
                Name = name,
                CancellationToken = cancellationToken
            };

            var result = await _mediator.Send (request);
            return StatusCode (result.StatusCode, result);
        }

        /// <summary>
        /// An endpoint to enable a logged in user request for all industries. Admin and staff can also use this endpoint to spool all industries that were created by a user.
        /// </summary>
        /// <param name="id">The userId of the user whose industries are being searched for</param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet ("industries")]
        [EnableRateLimiting ("GetRequestRateLimit")]
        [ProducesResponseType (type: typeof (RequestResponse<List<IndustryResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType (type: typeof (RequestResponse<List<IndustryResponse>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType (type: typeof (RequestResponse<List<IndustryResponse>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType (type: typeof (RequestResponse<List<IndustryResponse>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType (type: typeof (RequestResponse<List<IndustryResponse>>), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<RequestResponse<IndustryResponse>>> GetIndustries ([FromQuery] string? id, CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 10)
        {
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

            if (tokenResponse.UserRole == null)
            {
                return BadRequest (tokenResponse.Remark);
            }

            if (!tokenResponse.UserId.Equals (id, StringComparison.OrdinalIgnoreCase) && !tokenResponse.UserRole.Equals (UserRoles.Staff, StringComparison.OrdinalIgnoreCase) && id != null && !tokenResponse.UserRole.Equals (UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized ("You are unauthorized to make this request");
            }

            IndustriesQuery request = new ()
            {
                UserId = id,
                PageNumber = pageNumber,
                PageSize = pageSize,
                CancellationToken = cancellationToken
            };
            var result = await _mediator.Send (request);
            return StatusCode (result.StatusCode, result);
        }

        /// <summary>
        /// An endpoint to enable admin update some crucial parts of an existing industry
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut ("industry")]
        [Authorize (Roles = UserRoles.Admin)]
        [EnableRateLimiting ("PostRequestRateLimit")]
        [ProducesResponseType (type: typeof (RequestResponse<IndustryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<RequestResponse<IndustryResponse>>> UpdateIndustry ([FromBody] UpdateIndustryCommand request, CancellationToken cancellationToken)
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
        /// An endpoint to enable admin delete an existing industry
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete ("industry")]
        [Authorize (Roles = UserRoles.Admin)]
        [EnableRateLimiting ("PostRequestRateLimit")]
        [ProducesResponseType (type: typeof (RequestResponse<IndustryResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<RequestResponse<IndustryResponse>>> DeleteIndustry ([FromBody] DeleteIndustryCommand request, CancellationToken cancellationToken)
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
    }
}
