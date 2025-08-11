using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Application.Model;
using Application.Models.Transactions.Response;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Application.Models.Transactions.Command
{
	public class DepositCommand : IRequest<RequestResponse<TransactionResponse>>
	{
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? Description { get; set; }
		[Precision (18, 2)]
		[Range (0.01, double.MaxValue, ErrorMessage = "{0} must be greater than {1}.")]
		public decimal Amount { get; set; }
		[Required]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string RecipientAccountNumber { get; set; }
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? Notes { get; set; }


		/// <summary>
		/// The GUID userId of the person updating this record
		/// </summary>
		[JsonIgnore]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? CreatedBy { get; set; }
		[Required (ErrorMessage = "Transaction Currency is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Currency { get; set; } = "NGN";
		[JsonIgnore]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? PaymentReferenceId { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}