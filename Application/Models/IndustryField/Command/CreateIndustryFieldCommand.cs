using Application.Models.IndustryField.Response;

using MediatR;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Models.IndustryField.Command
{
    public class CreateIndustryFieldCommand : IRequest<RequestResponse<IndustryFieldResponse>>
    {
        [Required (ErrorMessage = "IndustryId is required")]
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
        public int Order { get; set; }
        [StringLength (200, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? ToolTip { get; set; }
        [JsonIgnore]
        public string? CreatedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
