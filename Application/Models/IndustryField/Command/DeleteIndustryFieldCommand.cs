using Application.Models.IndustryField.Response;

using MediatR;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.Models.IndustryField.Command
{
    public class DeleteIndustryFieldCommand : IRequest<RequestResponse<IndustryFieldResponse>>
    {
        /// Id of the industry field
        /// </summary>
        [Required (ErrorMessage = "Id is required")]
        public long Id { get; set; }
        /// <summary>
        /// Id of the user who is deleting the industry field
        /// </summary>
        [JsonIgnore]
        public string DeletedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
