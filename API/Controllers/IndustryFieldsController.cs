using API.Middleware;

using Application.Models;
using Application.Models.IndustryField.Command;
using Application.Models.IndustryField.Queries;
using Application.Models.IndustryField.Response;
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
    [ProducesResponseType (type: typeof (RequestResponse<IndustryFieldResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType (type: typeof (RequestResponse<IndustryFieldResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType (type: typeof (RequestResponse<IndustryFieldResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType (type: typeof (RequestResponse<IndustryFieldResponse>), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType (StatusCodes.Status500InternalServerError)]
    public class IndustryFieldsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public IndustryFieldsController (IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// An endpoint to enable a admin create an industry field
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost ("industry-field")]
        [Authorize (Roles = UserRoles.Admin)]
        [EnableRateLimiting ("PostRequestRateLimit")]
        [Idempotent (cacheTimeInSeconds: 60)]
        [ProducesResponseType (type: typeof (RequestResponse<IndustryFieldResponse>), StatusCodes.Status201Created)]
        public async Task<ActionResult<RequestResponse<IndustryFieldResponse>>> CreateIndustryField ([FromBody] CreateIndustryFieldCommand request, CancellationToken cancellationToken)
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
        /// An endpoint to enabled logged in users get information about an industry field
        /// </summary>
        /// <param name="userPublicId">This allows admin and staff to get the number of industry fields created by a specific user</param>
        /// <param name="id">This allows users to search for an industry field using its industry ID on the database</param>
        /// <param name="name">This allows users to search for an industry field using its name on the database</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet ("industry-field")]
        [EnableRateLimiting ("GetRequestRateLimit")]
        [ProducesResponseType (type: typeof (RequestResponse<IndustryFieldResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<RequestResponse<IndustryFieldResponse>>> GetIndustryField ([FromQuery] long? id, string? userPublicId, string? name, CancellationToken cancellationToken)
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

            IndustryFieldQuery request = new ()
            {
                UserId = userPublicId,
                Name = name,
                Id = id,
                CancellationToken = cancellationToken
            };

            var result = await _mediator.Send (request);
            return StatusCode (result.StatusCode, result);
        }

        /// <summary>
        /// An endpoint to enable a logged in user request for all industry fields. Admin and staff can also use this endpoint to spool all industry fields that were created by a user.
        /// </summary>
        /// <param name="id">The userId of the user whose industry fields are being searched for</param>
        /// <param name="industryId">The industryId of the industry whose fields are being searched for</param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet ("industry-fields")]
        [EnableRateLimiting ("GetRequestRateLimit")]
        [ProducesResponseType (type: typeof (RequestResponse<List<IndustryFieldResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType (type: typeof (RequestResponse<List<IndustryFieldResponse>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType (type: typeof (RequestResponse<List<IndustryFieldResponse>>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType (type: typeof (RequestResponse<List<IndustryFieldResponse>>), StatusCodes.Status404NotFound)]
        [ProducesResponseType (type: typeof (RequestResponse<List<IndustryFieldResponse>>), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<RequestResponse<IndustryFieldResponse>>> GetIndustryFields ([FromQuery] string? id, long? industryId, CancellationToken cancellationToken, int pageNumber = 1, int pageSize = 10)
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

            IndustryFieldsQuery request = new ()
            {
                UserId = id,
                IndustryId = industryId,
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
        [HttpPut ("industry-field")]
        [Authorize (Roles = UserRoles.Admin)]
        [EnableRateLimiting ("PostRequestRateLimit")]
        [ProducesResponseType (type: typeof (RequestResponse<IndustryFieldResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<RequestResponse<IndustryFieldResponse>>> UpdateIndustryField ([FromBody] UpdateIndustryFieldCommand request, CancellationToken cancellationToken)
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
        [HttpDelete ("industry-field")]
        [Authorize (Roles = UserRoles.Admin)]
        [EnableRateLimiting ("PostRequestRateLimit")]
        [ProducesResponseType (type: typeof (RequestResponse<IndustryFieldResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<RequestResponse<IndustryFieldResponse>>> DeleteIndustryField ([FromBody] DeleteIndustryFieldCommand request, CancellationToken cancellationToken)
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
