using Application.Models.Accounts.Response;

using MediatR;

namespace Application.Models.Accounts.Queries
{
    public class AccountsQuery : IRequest<RequestResponse<List<AccountResponse>>>
    {
        public string PublicId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }

}
