using System.ComponentModel.DataAnnotations;

namespace Application.Models.Branches.Command
{
    public class DeleteBranchCommand
    {
        /// <summary>
        /// Id of the branch
        /// </summary>
        [Required (ErrorMessage = "Id is required")]
        public string PublicId { get; set; }
        /// <summary>
        /// Id of the user who is deleting the branch
        /// </summary>
        [Required (ErrorMessage = "DeletedBy is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string DeletedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
