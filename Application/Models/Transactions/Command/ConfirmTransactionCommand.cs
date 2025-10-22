using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Application.Models.Transactions.Response;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Application.Models.Transactions.Command
{
	public class ConfirmTransactionCommand : IRequest<RequestResponse<TransactionResponse>>
	{
		[Required (ErrorMessage = "Payment Reference ID is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PaymentReferenceId { get; set; }
		[Precision (18, 2)]
		[Range (0.01, double.MaxValue, ErrorMessage = "{0} must be greater than {1}.")]
		public decimal Amount { get; set; }
		[JsonIgnore]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? LastModifiedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
