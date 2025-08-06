using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public class Wallet : AuditableEntity
	{
		/// <summary>
		/// Unique GUID string of the wallet
		/// </summary>
		[Required (ErrorMessage = "PublicId is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PublicId { get; set; }

		/// <summary>
		/// Unique GUID string of the account detail
		/// </summary>
		[Required (ErrorMessage = "PublicId is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string AccountDetailPublicId { get; set; }

		[Required (ErrorMessage = "PublicId is required")]
		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string AccountNumber { get; set; }
		/// <summary>
		/// This should contain information on how much a business or user has with us
		/// </summary>
		[Range (0.01, double.MaxValue, ErrorMessage = "{0} must be greater than {1}.")]
		public decimal Balance { get; set; }

		/// <summary>
		/// The most recent transaction (debit or credit)
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? MostRecentTransactionType { get; set; }

		/// <summary>
		/// The name of the most recent channel of transaction (bank transfer, or debit as a result of a service consumed)
		/// </summary>
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? MostRecentPaymentService { get; set; }

		[Range (0.01, double.MaxValue, ErrorMessage = "{0} must be greater than {1}.")]
		public decimal MostRecentTransactionAmount { get; set; }

		[StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? NameOfLastTransactionBeneficiary { get; set; }
	}
}
