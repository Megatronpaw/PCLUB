using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PClub.Api.Infrastructure
{
    /// <summary>
    /// Последний в цепочке: ловит всё, что не предусмотрено.
    /// </summary>
    public sealed class UnhandledExceptionHandler : IExceptionHandler
    {
        /// <summary>
        /// Код "клиент закрыл соединение". В StatusCodes его нет.
        /// </summary>
        private const int ClientClosedRequest = 499;

        private readonly IProblemDetailsService _problemDetails;
        private readonly ILogger<UnhandledExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        /// <summary>
        /// Создаёт обработчик
        /// </summary>
        public UnhandledExceptionHandler(
            IProblemDetailsService problemDetails,
            ILogger<UnhandledExceptionHandler> logger,
            IHostEnvironment environment)
        {
            _problemDetails = problemDetails;
            _logger = logger;
            _environment = environment;
        }

        ///<inheritdoc/>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Запрос отменён клиентом: {Path}", httpContext.Request.Path);
                httpContext.Response.StatusCode = ClientClosedRequest;
                
                return true;
            }

            _logger.LogError(
                exception, "Необработанное исключение: {Path}", httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Внутренняя ошибка",

                    Detail = _environment.IsDevelopment()
                        ? exception.ToString()
                        : "Что-то пошло не так. Сообщите traceId в поддерждку."

                }
            });
        }
    }
}
