using Application.Model.AccountDetails.Command;
using Application.Model.Uploads.Queries;
using Application.Models.Banks.Response;
using Application.Models.Branches.Response;
using Application.Models.Payments.Response;
using Application.Models.Transactions.Response;
using Application.Models.Users.Response;

namespace Application.Models.AuditLogs.Response
{
	public class AuditLogsQueryResponse
	{
		public List<AccountDetailResponse>? AccountDetailLogs { get; set; }
		public List<BankResponse>? BankLogs { get; set; }
		public List<BranchResponse>? BranchLogs { get; set; }
		public List<PaymentResponse>? PaymentLogs { get; set; }
		public List<TransactionResponse>? TransactionLogs { get; set; }
		public List<UploadResponse>? UploadLogs { get; set; }
		public List<UserResponse>? UserLogs { get; set; }
	}
}
