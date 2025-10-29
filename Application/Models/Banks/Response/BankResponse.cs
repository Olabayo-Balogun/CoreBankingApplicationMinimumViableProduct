using System.ComponentModel.DataAnnotations;

namespace Application.Models.Banks.Response
{
    public class BankResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string NibssCode { get; set; }
        /// <summary>
        /// The CBN bank's short code/abbreviation
        /// </summary>
        [Required (ErrorMessage = "Bank code is required")]
        [StringLength (50, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string CbnCode { get; set; }
    }
}