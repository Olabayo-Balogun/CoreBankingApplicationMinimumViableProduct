using Application.Models.Accounts.Response;
using Application.Models.Industry.Response;

using MediatR;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Models.Industry.Command
{
    public class CreateIndustryCommand : IRequest<RequestResponse<IndustryResponse>>
    {
        [Required (ErrorMessage = "Name is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Name { get; set; }
        [StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }
        [JsonIgnore]
        public string? CreatedBy { get; set; }
    }
}
