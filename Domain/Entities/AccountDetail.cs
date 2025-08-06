using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace Domain.Entities
{
	public class AccountDetail : AuditableEntity
	{
		/// <summary>
		/// The publicId is a unique GUID that's used to point directly to the account detail on the DB
		/// </summary>
		[Required (ErrorMessage = "PublicId is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PublicId { get; set; }

		/// <summary>
		/// The account number of the account detail
		/// </summary>
		[Required (ErrorMessage = "Account number is required")]
		[StringLength (50, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string AccountNumber { get; set; }

		/// <summary>
		/// This is used to identify whether the account is savings, current, or any other type of account
		/// </summary>
		[Required]
		public int AccountType { get; set; }

		/// <summary>
		/// This is used to set the status of the account, whether it's active, inactive, closed, or PND
		/// </summary>
		[Required]
		public int AccountStatus { get; set; }

		/// <summary>
		/// This is an internal number used to identify the account in the bank's ledger, it can be used to quickly determine the branch name, currency code, and other information of the account
		/// </summary>
		[Required]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string LedgerNumber { get; set; }

		[Required]
		[Range (0.01, double.MaxValue, ErrorMessage = "{0} must be greater than {1}.")]
		[Precision (18, 2)]
		public decimal MaximumDailyWithdrawalLimitAmount { get; set; }
	}
}
