using System.ComponentModel.DataAnnotations;

namespace Domain
{
	public class AuditableEntity
	{
		public long Id { get; set; }
		public bool IsDeleted { get; set; }
		[Required]
		public DateTime DateCreated { get; set; }
		public DateTime? LastModifiedDate { get; set; }
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? LastModifiedBy { get; set; }
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public DateTime? DateDeleted { get; set; }
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string? DeletedBy { get; set; }
		[Required]
		[StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
		public string CreatedBy { get; set; }
	}
}