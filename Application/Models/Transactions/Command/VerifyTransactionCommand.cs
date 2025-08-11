using System.ComponentModel.DataAnnotations;

using Application.Model;
using Application.Models.Transactions.Response;

using MediatR;

namespace Application.Models.Transactions.Command
{
	public class VerifyTransactionCommand : IRequest<RequestResponse<TransactionResponse>>
	{
		[Required (ErrorMessage = "Payment Reference ID is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PaymentReferenceId { get; set; }
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string LastModifiedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
