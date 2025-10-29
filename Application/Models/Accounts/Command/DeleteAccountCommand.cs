using Application.Models.Accounts.Response;

using MediatR;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Models.Accounts.Command
{
    public class DeleteAccountCommand : IRequest<RequestResponse<AccountResponse>>
    {
        /// <summary>
        /// Id of the account
        /// </summary>
        [Required (ErrorMessage = "Id is required")]
        public string Id { get; set; }
        /// <summary>
        /// Id of the user who is deleting the account
        /// </summary>
        [JsonIgnore]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? DeletedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
