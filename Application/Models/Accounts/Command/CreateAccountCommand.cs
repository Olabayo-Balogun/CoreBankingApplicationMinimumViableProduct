using System.ComponentModel.DataAnnotations;

namespace Application.Models.Accounts.Command
{
	public class CreateAccountCommand
	{
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

		public string CreatedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
