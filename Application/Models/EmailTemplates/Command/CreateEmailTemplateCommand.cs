using System.ComponentModel.DataAnnotations;

namespace Application.Models.EmailTemplates.Command
{
    public class CreateEmailTemplateCommand
    {
        /// <summary>
        /// The name of the email template that you're creating
        /// </summary>
        [Required (ErrorMessage = "Template name cannot be empty")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string TemplateName { get; set; }
        /// <summary>
        /// The name of the channel of the email template
        /// </summary>
        [Required (ErrorMessage = "Channel name cannot be empty")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Channel { get; set; }
        /// <summary>
        /// The email template
        /// </summary>
        [StringLength (200000, ErrorMessage = "Input must not exceed 200000 characters")]
        [Required (ErrorMessage = "Template cannot be empty")]
        public string Template { get; set; }
        /// <summary>
        /// UserPublicId of the user initiating this request.
        /// </summary>
        [Required (ErrorMessage = "UserPublicId cannot be empty")]
        public string UserId { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
