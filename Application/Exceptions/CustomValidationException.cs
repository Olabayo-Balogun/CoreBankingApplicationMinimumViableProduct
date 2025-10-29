using FluentValidation.Results;

namespace Application.Exceptions
{
    public class CustomValidationException : ApplicationException
    {
        public List<string> validationErrors { get; set; }

        public CustomValidationException (ValidationResult validationResult)
        {
            validationErrors = [];
            foreach (var validationError in validationResult.Errors)
            {
                validationErrors.Add (validationError.ErrorMessage);
            }
        }
    }
}
