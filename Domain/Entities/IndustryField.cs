using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class IndustryField : AuditableEntity
    {
        [Required (ErrorMessage = "IndustryId is required")]
        [Range (1, long.MaxValue, ErrorMessage = "IndustryId must be at least {1}.")]
        public long IndustryId { get; set; }
        [Required (ErrorMessage = "Name is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Name { get; set; }
        [Required (ErrorMessage = "DataType is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string DataType { get; set; }
        [Required (ErrorMessage = "IsRequired is required")]
        public bool IsRequired { get; set; }
        [Required (ErrorMessage = "Order is required")]
        [Range (1, int.MaxValue, ErrorMessage = "Order must be at least {1}.")]
        public int Order { get; set; }
        [StringLength (200, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? ToolTip { get; set; }
    }
}
