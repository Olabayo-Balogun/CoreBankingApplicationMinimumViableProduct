using Application.Models.Transactions.Response;

using MediatR;

using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace Application.Models.Transactions.Queries
{
    public class TransactionsQuery : IRequest<RequestResponse<List<TransactionResponse>>>
    {
        public string? AccountNumber { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public string? UserId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? Week { get; set; }
        public DateTime? Month { get; set; }
        public DateTime? Year { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [Precision (18, 2)]
        [Range (0.01, double.MaxValue, ErrorMessage = "{0} must be greater than {1}.")]
        public decimal? Amount { get; set; }
    }
}
