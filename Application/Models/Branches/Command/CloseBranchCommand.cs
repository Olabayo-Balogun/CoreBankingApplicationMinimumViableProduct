using System.ComponentModel.DataAnnotations;

namespace Application.Models.Branches.Command
{
	public class CloseBranchCommand
	{
		/// <summary>
		/// Id of the branch
		/// </summary>
		[Required (ErrorMessage = "Id is required")]
		public string Id { get; set; }
		/// <summary>
		/// Id of the user who is closing the branch
		/// </summary>
		[Required (ErrorMessage = "LastModifiedBy is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string LastModifiedBy { get; set; }
		public CancellationToken CancellationToken { get; set; }
	}
}
