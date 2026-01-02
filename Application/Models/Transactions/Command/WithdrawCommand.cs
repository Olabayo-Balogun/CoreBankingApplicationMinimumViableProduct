using Application.Models.Transactions.Response;

using MediatR;

using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.Models.Transactions.Command
{
    public class WithdrawCommand : IRequest<RequestResponse<TransactionResponse>>
    {
        [StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }
        [Precision (18, 2)]
        [Range (0.01, double.MaxValue, ErrorMessage = "{0} must be greater than {1}.")]
        public decimal Amount { get; set; }
        [Required]
        [StringLength (1000, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string AccountNumber { get; set; }
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? Notes { get; set; }
        /// <summary>
        /// The GUID userId of the person updating this record
        /// </summary>
        [JsonIgnore]
        [StringLength (100, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? CreatedBy { get; set; }
        [Required (ErrorMessage = "Transaction Currency is required")]
        [StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Currency { get; set; } = "NGN";
        [JsonIgnore]
        [StringLength (500, ErrorMessage = "{0} must be at least {2} characters long.", MinimumLength = 2)]
        public string? PaymentReferenceId { get; set; }
        public Dictionary<string, string>? MetaData { get; set; }
        public CancellationToken CancellationToken { get; set; }


        private const int MaxMetaEntries = 100;
        private const int MaxMetaKeyLength = 200;
        private const int MaxMetaValueLength = 1000;

        public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
        {
            if (MetaData == null)
            {
                yield break;
            }

            if (MetaData.Count > MaxMetaEntries)
            {
                yield return new ValidationResult ($"MetaData cannot contain more than {MaxMetaEntries} entries.", new[] { nameof (MetaData) });
            }

            // Build a cleaned dictionary and detect duplicates produced by trimming
            var cleaned = new Dictionary<string, string> (StringComparer.Ordinal);
            foreach (var kv in MetaData)
            {
                if (kv.Key == null)
                {
                    yield return new ValidationResult ("MetaData contains a null key.", new[] { nameof (MetaData) });
                    continue;
                }

                var trimmedKey = kv.Key.Trim ();
                if (string.IsNullOrEmpty (trimmedKey))
                {
                    yield return new ValidationResult ("MetaData contains an empty key.", new[] { nameof (MetaData) });
                    continue;
                }

                if (trimmedKey.Length > MaxMetaKeyLength)
                {
                    yield return new ValidationResult ($"MetaData key '{trimmedKey}' exceeds maximum length of {MaxMetaKeyLength}.", new[] { nameof (MetaData) });
                    continue;
                }

                var trimmedValue = kv.Value?.Trim () ?? string.Empty;
                if (trimmedValue.Length > MaxMetaValueLength)
                {
                    yield return new ValidationResult ($"MetaData value for key '{trimmedKey}' exceeds maximum length of {MaxMetaValueLength}.", new[] { nameof (MetaData) });
                    continue;
                }

                if (cleaned.ContainsKey (trimmedKey))
                {
                    yield return new ValidationResult ($"MetaData contains duplicate key after trimming: '{trimmedKey}'.", new[] { nameof (MetaData) });
                    continue;
                }

                cleaned[trimmedKey] = trimmedValue;
            }

            // If there were no validation errors regarding trimming/duplicates, replace MetaData with the trimmed version.
            // (ModelState will contain any ValidationResults yielded above)
            if (!validationContext.Items.ContainsKey ("__HasMetaDataValidationErrors"))
            {
                // We cannot examine ModelState here reliably; set cleaned only when it's safe:
                // replace regardless — duplicates/other errors are already reported and will cause request rejection.
                MetaData = cleaned;
            }
        }
    }
}
