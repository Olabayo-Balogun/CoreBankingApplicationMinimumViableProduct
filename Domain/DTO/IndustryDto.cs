using System.ComponentModel.DataAnnotations;

namespace Domain.DTO
{
    public class IndustryDto : AuditableEntityDto
    {
        [Required (ErrorMessage = "Name is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Name { get; set; }
        [StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
