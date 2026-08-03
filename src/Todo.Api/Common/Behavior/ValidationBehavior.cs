using FluentValidation;
using MediatR;

namespace Todo.Api.Common.Behavior
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull // IRequest<TResponse> là cho bản cũng 11- DÙng để chỉ nhận những thằng đã kế thừa IRequest mới vào validation này được.
        // ICommandBase : Của ông Millan gì đó trên doc. Ông tự tạo 1 class riêng. Nên không làm theo cũng được. Basic đc r.
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any()) return await next();

            // Lấy request để check
            var context = new ValidationContext<TRequest>(request);

            var validationFailures = await Task.WhenAll(
                _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));

            var errors = validationFailures
                .SelectMany(result => result.Errors)
                .Where(failure => failure != null)
                .ToList();

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }

            return await next();
        }
    }
}
