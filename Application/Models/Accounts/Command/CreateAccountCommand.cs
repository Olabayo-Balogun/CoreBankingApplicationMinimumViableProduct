using System.ComponentModel.DataAnnotations;

using Application.Model;
using Application.Models.Accounts.Response;

using MediatR;

namespace Application.Models.Accounts.Command
{
	public class CreateAccountCommand : IRequest<RequestResponse<AccountResponse>>
	{
		/// <summary>
		/// This is used to identify whether the account is savings, current, or any other type of account
		/// </summary>
		[Required]
		[Range (1, 10, ErrorMessage = "{0} must be a valid account type.")]
		public int AccountType { get; set; }

		public string? CreatedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
