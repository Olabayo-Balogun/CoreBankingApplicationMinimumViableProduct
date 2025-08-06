using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public class Branch : AuditableEntity
	{
		[Required (ErrorMessage = "PublicId is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string PublicId { get; set; }
		[Required (ErrorMessage = "Name is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Name { get; set; }
		public int Code { get; set; }
		[Required (ErrorMessage = "Address is required")]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Address { get; set; }
		[Required (ErrorMessage = "Lga is required")]
		[StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Lga { get; set; }
		[Required (ErrorMessage = "State is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string State { get; set; }
		[Required (ErrorMessage = "Country is required")]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string Country { get; set; }
		public bool IsClosed { get; set; }
	}
}
