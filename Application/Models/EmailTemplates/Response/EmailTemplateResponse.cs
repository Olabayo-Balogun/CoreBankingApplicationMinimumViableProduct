using System.ComponentModel.DataAnnotations;

namespace Application.Models.EmailTemplates.Response
{
	public class EmailTemplateResponse
	{
		public long Id { get; set; }
		[Required (ErrorMessage = "Template name cannot be empty")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string TemplateName { get; set; }
		[Required (ErrorMessage = "Channel name cannot be empty")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Channel { get; set; }
		[Required (ErrorMessage = "Template cannot be empty")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Template { get; set; }
	}
}
