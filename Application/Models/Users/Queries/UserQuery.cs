using System.ComponentModel.DataAnnotations;

using Application.Model;
using Application.Models.Users.Response;

using MediatR;

namespace Application.Models.Users.Queries
{
	public class UserQuery : IRequest<RequestResponse<UserResponse>>
	{
		public string? Period { get; set; }
		public string? UserPublicId { get; set; }
		public bool IsCount { get; set; }
		public DateTime? Date { get; set; }
		public bool? IsDeleted { get; set; }
		public string? Role { get; set; }
		[EmailAddress]
		public string? EmailAddress { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
