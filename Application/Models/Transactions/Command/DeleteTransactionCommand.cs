using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Application.Model;
using Application.Models.Transactions.Response;

using MediatR;

namespace Application.Models.Transactions.Command
{
	public class DeleteTransactionCommand : IRequest<RequestResponse<TransactionResponse>>
	{
		[Required]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PublicId { get; set; }
		[JsonIgnore]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}