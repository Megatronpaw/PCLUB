using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PClub.Application.Auth;

namespace PClub.Api.Controllers;

/// <summary>Регистрация, вход и обновление токенов.</summary>
public sealed class AuthController : ApiControllerBase
{
    private readonly IAuthService _auth;

    /// <summary>Создаёт контроллер.</summary>
    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Регистрирует пользователя и сразу выдаёт токены.</summary>
    /// <param name="request">Почта, пароль и имя.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">Пользователь создан, токены выданы.</response>
    /// <response code="400">Данные не прошли проверку.</response>
    /// <response code="409">Почта уже занята.</response>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResultDto>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _auth.RegisterAsync(request, cancellationToken));

    /// <summary>Вход по почте и паролю.</summary>
    /// <param name="request">Почта и пароль.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">Токены выданы.</response>
    /// <response code="401">Неверная почта или пароль.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _auth.LoginAsync(request, cancellationToken));

    /// <summary>Обменивает токен обновления на новую пару.</summary>
    /// <param name="request">Токен обновления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">Новая пара токенов.</response>
    /// <response code="401">Токен недействителен или уже использован.</response>
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResultDto>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _auth.RefreshAsync(request, cancellationToken));

    /// <summary>Завершает все сеансы этой цепочки.</summary>
    /// <param name="request">Токен обновления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="204">Сеанс завершён. Повторный вызов тоже 204.</response>
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        await _auth.LogoutAsync(request.RefreshToken, cancellationToken);

        return NoContent();
    }

    /// <summary>Возвращает данные текущего пользователя.</summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">Данные пользователя.</response>
    /// <response code="401">Вход не выполнен.</response>
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me(
        CancellationToken cancellationToken) =>
        Ok(await _auth.GetCurrentUserAsync(cancellationToken));
}
