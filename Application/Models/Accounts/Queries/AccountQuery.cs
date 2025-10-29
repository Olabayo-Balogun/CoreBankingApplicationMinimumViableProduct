using Application.Models.Accounts.Response;

using MediatR;

namespace Application.Models.Accounts.Queries
{
    public class AccountQuery : IRequest<RequestResponse<AccountResponse>>
    {
        public string? PublicId { get; set; }
        public string? UserPublicId { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountLedger { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
