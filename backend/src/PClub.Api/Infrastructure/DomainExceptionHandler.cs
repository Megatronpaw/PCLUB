using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PClub.Domain.Exceptions;

namespace PClub.Api.Infrastructure
{
    /// <summary>
    /// Превращает исключения предметной области в ответы ProblemDetails.
    /// </summary>
    /// <remarks>
    /// Один этот класс заменяет все try/catch в контроллерах. Добавил новый тип
    /// исключения — дописал строку в switch, и он работает во всех эндпоинтах сразу.
    /// </remarks>
    public sealed class DomainExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;
        private readonly ILogger<DomainExceptionHandler> _logger;

        /// <summary>Создаёт обработчик.</summary>
        /// <param name="problemDetails">Служба, пишущая ответ в формате ProblemDetails.</param>
        /// <param name="logger">Журнал.</param>
        public DomainExceptionHandler(
            IProblemDetailsService problemDetails,
            ILogger<DomainExceptionHandler> logger)
        {
            _problemDetails = problemDetails;
            _logger = logger;
        }

        /// <inheritdoc />
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not DomainException domainException)
            {
                return false;
            }

            var (statusCode, title) = Map(domainException);

           
            _logger.LogInformation(
                "Отказ предметной области: {ExceptionType} → {StatusCode}. {Message}",
                domainException.GetType().Name, statusCode, domainException.Message);

            httpContext.Response.StatusCode = statusCode;

            return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = domainException,
                ProblemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,

                    Detail = domainException.Message,
                },
            });
        }

        /// <summary>Сопоставляет тип исключения с кодом HTTP.</summary>
        /// <param name="exception">Исключение предметной области.</param>
        private static (int StatusCode, string Title) Map(DomainException exception) =>
            exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Не найдено"),
                ConflictException => (StatusCodes.Status409Conflict, "Конфликт"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Запрещено"),
                DomainValidationException => (StatusCodes.Status400BadRequest, "Неверные данные"),

                _ => (StatusCodes.Status422UnprocessableEntity, "Правило нарушено"),
            };
    }
}
