using System.ComponentModel.DataAnnotations;

namespace Application.Models.Accounts.Command
{
	public class DeleteAccountCommand
	{
		/// <summary>
		/// Id of the account
		/// </summary>
		[Required (ErrorMessage = "Id is required")]
		public string Id { get; set; }
		/// <summary>
		/// Id of the user who is deleting the account
		/// </summary>
		[Required (ErrorMessage = "DeletedBy is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
