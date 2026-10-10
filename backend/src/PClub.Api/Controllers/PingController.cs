using Microsoft.AspNetCore.Mvc;
using PClub.Application.Abstractions;
using PClub.Domain.Exceptions;

namespace PClub.Api.Controllers;

/// <summary>Проверка, что сервис поднялся и отвечает.</summary>
public sealed class PingController : ApiControllerBase
{
    private readonly IClock _clock;

    /// <summary>Создаёт контроллер.</summary>
    /// <param name="clock">Текущее время.</param>
    public PingController(IClock clock) => _clock = clock;

    /// <summary>Возвращает время сервера в UTC.</summary>
    /// <response code="200">Сервис жив.</response>
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        status = "ok",

        utc = _clock.UtcNow,
    });

    /// <summary>Возвращает твоё же сообщение.</summary>
    /// <param name="text">Текст для возврата.</param>
    /// <response code="200">Текст принят.</response>
    /// <response code="400">Текст пустой.</response>
    [HttpGet("echo")]
    public IActionResult Echo(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return BadRequest("Нужно ввести хоть что-то");
        }

        return Ok(new { youSaid = text, length = text.Length });
    }
}
