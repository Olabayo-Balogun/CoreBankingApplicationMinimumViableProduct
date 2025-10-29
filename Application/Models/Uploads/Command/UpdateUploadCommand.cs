using Microsoft.AspNetCore.Http;

using System.ComponentModel.DataAnnotations;

namespace Application.Models.Uploads.Command
{
    public class UpdateUploadCommand
    {

        /// <summary>
        /// The ID of the upload you want to update
        /// </summary>
        [Required (ErrorMessage = "Upload valid PublicId")]
        public string PublicId { get; set; }
        /// <summary>
        /// The upload file
        /// </summary>
        [Required (ErrorMessage = "Upload valid file")]
        public IFormFile UploadFile { get; set; }
        /// <summary>
        /// The userId of the person uploading the file
        /// </summary>
        [Required (ErrorMessage = "Input UserPublicId")]
        public string LastModifiedBy { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
