using System.ComponentModel.DataAnnotations;

namespace Application.Models.EmailLogs.Command
{
    public class CreateEmailLogCommand
    {
        /// <summary>
        /// Email address of the recipient of the email
        /// </summary>
        [EmailAddress (ErrorMessage = "Email address of recipient is required")]
        [StringLength (100, ErrorMessage = "Input must not exceed 100 characters")]
        public string ToRecipient { get; set; }
        /// <summary>
        /// The email address of the person being copied in the email
        /// </summary>
        [StringLength (100, ErrorMessage = "Input must not exceed 100 characters")]
        public string? CcRecipient { get; set; }
        /// <summary>
        /// The email address of the person being blind copied in the email
        /// </summary>
        [StringLength (100, ErrorMessage = "Input must not exceed 100 characters")]
        public string? BccRecipient { get; set; }
        /// <summary>
        /// The subject of the email
        /// </summary>
        [StringLength (100, ErrorMessage = "Input must not exceed 100 characters")]
        public string? Subject { get; set; }

        /// <summary>
        /// The message to be sent
        /// </summary>
        [StringLength (200000, ErrorMessage = "Input must not exceed 200000 characters")]
        [Required (ErrorMessage = "Message is required")]
        public string Message { get; set; }
        /// <summary>
        /// Is the message to be sent written as HTML? 
        /// </summary>
        [Required (ErrorMessage = "HTML status is required")]
        public bool IsHtml { get; set; }
        /// <summary>
        /// UserPublicId of the user initiating this request.
        /// </summary>

        [Required (ErrorMessage = "UserPublicId cannot be empty")]
        public string CreatedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
