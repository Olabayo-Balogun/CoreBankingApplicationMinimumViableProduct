using System.ComponentModel.DataAnnotations;

namespace Application.Model.EmailLogs.Queries
{
	public class EmailLogResponse
	{
		public long Id { get; set; }
		[EmailAddress (ErrorMessage = "Email address of recipient cannot be empty")]
		public string ToRecipient { get; set; }
		public string? CcRecipient { get; set; }
		public string? BccRecipient { get; set; }
		public string? Subject { get; set; }
		[Required (ErrorMessage = "Message cannot be empty")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Message { get; set; }
		[EmailAddress (ErrorMessage = "Email address of sender cannot be empty")]
		public string Sender { get; set; }
		[Required (ErrorMessage = "HTML status is required")]
		public bool IsHtml { get; set; }
		[Required (ErrorMessage = " Sent status is required")]
		public bool IsSent { get; set; }
		public DateTime? DateSent { get; set; }
	}
}
