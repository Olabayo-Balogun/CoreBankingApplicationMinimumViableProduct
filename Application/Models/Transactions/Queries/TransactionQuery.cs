using Application.Model;
using Application.Models.Transactions.Response;

using MediatR;

namespace Application.Models.Transactions.Queries
{
	public class TransactionQuery : IRequest<RequestResponse<TransactionResponse>>
	{
		public string? AccountNumber { get; set; }
		public string? PublicId { get; set; }
		public CancellationToken CancellationToken { get; set; }
		public string? UserId { get; set; }
		public DateTime? Date { get; set; }
		public DateTime? Week { get; set; }
		public DateTime? Month { get; set; }
		public DateTime? Year { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
	}
}
