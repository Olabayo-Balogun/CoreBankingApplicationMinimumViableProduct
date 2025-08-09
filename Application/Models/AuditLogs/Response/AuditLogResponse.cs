using Application.Model.AccountDetails.Command;
using Application.Model.Uploads.Queries;
using Application.Models.Banks.Response;
using Application.Models.Branches.Response;
using Application.Models.Transactions.Response;
using Application.Models.Users.Response;

namespace Application.Models.AuditLogs.Response
{
	public class AuditLogResponse
	{
		public AccountDetailResponse? AccountLog { get; set; }
		public BankResponse? BankLog { get; set; }
		public BranchResponse? BranchLog { get; set; }
		public TransactionResponse? TransactionLog { get; set; }
		public UploadResponse? UploadLog { get; set; }
		public UserResponse? UserLog { get; set; }
	}
}
