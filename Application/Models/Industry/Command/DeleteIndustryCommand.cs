using Application.Models.Industry.Response;

using MediatR;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Models.Industry.Command
{
    public class DeleteIndustryCommand : IRequest<RequestResponse<IndustryResponse>>
    {
        /// Id of the industry
        /// </summary>
        [Required (ErrorMessage = "Id is required")]
        public long Id { get; set; }
        /// <summary>
        /// Id of the user who is deleting the industry
        /// </summary>
        [Required (ErrorMessage = "DeletedBy is required")]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string DeletedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
