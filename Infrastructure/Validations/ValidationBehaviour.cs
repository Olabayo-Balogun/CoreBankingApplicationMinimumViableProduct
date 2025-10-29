using Application.Exceptions;

using FluentValidation;

using MediatR;

namespace Infrastructure.Validations
{
    public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehaviour (IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        //public async Task<TResponse> Handle (TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        //{
        //	if (_validators.Any ())
        //	{
        //		var context = new ValidationContext<TRequest> (request);
        //		var validationResults = await Task.WhenAll (_validators.Select (v => v.ValidateAsync (context, cancellationToken)));
        //		if (validationResults.First ().Errors.Any ())
        //		{
        //			throw new CustomValidationException (validationResults.First ());
        //		}
        //	}
        //	return await next ();
        //}

        public async Task<TResponse> Handle (TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any ())
            {
                var context = new ValidationContext<TRequest> (request);
                var validationResults = await Task.WhenAll (_validators.Select (v => v.ValidateAsync (context, cancellationToken)));
                if (validationResults.First ().Errors.Any ())
                {
                    throw new CustomValidationException (validationResults.First ());
                }
            }
            return await next ();
        }
    }
}
