using FluentValidation;
using MediatR;
using System.Linq;
using ValidationException = Ordering.Application.Exceptions.ValidationException;

namespace Ordering.Application.Behaviours
{
    public class ValidationBehaviour<IRequest, IResponse> : IPipelineBehavior<IRequest, IResponse> where IRequest : MediatR.IRequest<IResponse>
    {
        private readonly IEnumerable<IValidator<IRequest>> _validators;

        public ValidationBehaviour(IEnumerable<IValidator<IRequest>> validators)
        {
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        }

        public Task<IResponse> Handle(IRequest request, RequestHandlerDelegate<IResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<IRequest>(request);

                var validationResults =  Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.Result.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Count != 0)
                    throw new ValidationException(failures);
            }

            return  next();
        }
    }
}