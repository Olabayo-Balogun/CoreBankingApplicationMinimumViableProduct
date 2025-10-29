using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class EmailRequest : AuditableEntity
    {
        [EmailAddress (ErrorMessage = "Email address of recipient cannot be empty")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string ToRecipient { get; set; }
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? CcRecipient { get; set; }
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? BccRecipient { get; set; }
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? Subject { get; set; }
        [Required (ErrorMessage = "Message cannot be empty")]
        public string Message { get; set; }
        [Required (ErrorMessage = "HTML status is required")]
        public bool IsHtml { get; set; }
        /// <summary>
        /// Does the message have an attachment 
        /// </summary>
        [Required (ErrorMessage = "HasAttachment is required")]
        public bool HasAttachment { get; set; }
        /// <summary>
        /// The Base64 string of the attachment
        /// </summary>
        public string? AttachmentBase64String { get; set; }
    }
}
