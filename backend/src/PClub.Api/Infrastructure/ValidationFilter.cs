using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PClub.Api.Infrastructure
{
    /// <summary>
    /// Проверяет аргументы действия всеми подходящими валидаторами.
    /// </summary>
    /// <remarks>
    /// Альтернатива — вызывать валидатор в начале каждого метода контроллера.
    /// Это работает, но повторяется, и однажды кто-нибудь забудет. Фильтр
    /// применяется ко всем действиям сразу, и забыть его нельзя.
    /// </remarks>
    public sealed class ValidationFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// Создаёт фильтр.
        /// </summary>
        /// <param name="services">Контейнер — из него достаются валидаторы.</param>
        public ValidationFilter(IServiceProvider services) => _services = services;

        /// <inheritdoc />
        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var errors = new Dictionary<string, List<string>>();

            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null)
                {
                    continue;
                }

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

                if (_services.GetService(validatorType) is not IValidator validator)
                {
                    continue;
                }

                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(
                    validationContext, context.HttpContext.RequestAborted);

                foreach (var failure in result.Errors)
                {
                    if (!errors.TryGetValue(failure.PropertyName, out var messages))
                    {
                        messages = [];
                        errors[failure.PropertyName] = messages;
                    }

                    messages.Add(failure.ErrorMessage);
                }
            }

            if (errors.Count > 0)
            {
                context.Result = new BadRequestObjectResult(
                    new ValidationProblemDetails(
                        errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray()))
                    {
                        Title = "Запрос не прошёл проверку",
                        Status = StatusCodes.Status400BadRequest,
                    });

                return;
            }

            await next();
        }
    }
}
