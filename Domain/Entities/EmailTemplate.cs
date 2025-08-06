using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public class EmailTemplate : AuditableEntity
	{
		[Required (ErrorMessage = "Template name cannot be empty")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string TemplateName { get; set; }
		[Required (ErrorMessage = "Channel name cannot be empty")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Channel { get; set; }
		[Required (ErrorMessage = "Template cannot be empty")]
		public string Template { get; set; }
	}
}
