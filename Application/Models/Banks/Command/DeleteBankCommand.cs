using System.ComponentModel.DataAnnotations;

namespace Application.Model.Banks.Command
{
	public class DeleteBankCommand
	{
		/// <summary>
		/// Id of the bank
		/// </summary>
		[Required (ErrorMessage = "Id is required")]
		public long Id { get; set; }
		/// <summary>
		/// Id of the user who is deleting the bank
		/// </summary>
		[Required (ErrorMessage = "DeletedBy is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string DeletedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}