using System.ComponentModel.DataAnnotations;

namespace Domain.DTO
{
    public class BankDto : AuditableEntityDto
    {
        [Required (ErrorMessage = "Bank name is required")]
        [StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Name { get; set; }
        /// <summary>
        /// The bank's short code/abbreviation
        /// </summary>
        [Required (ErrorMessage = "NibssCode is required")]
        [StringLength (50, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string NibssCode { get; set; }
        /// <summary>
        /// The CBN bank's short code/abbreviation
        /// </summary>
        [Required (ErrorMessage = "CbnCode is required")]
        [StringLength (50, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string CbnCode { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
