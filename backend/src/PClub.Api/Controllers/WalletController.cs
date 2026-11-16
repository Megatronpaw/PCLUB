using Microsoft.AspNetCore.Mvc;
using PClub.Application.Common;
using PClub.Application.Wallets;

namespace PClub.Api.Controllers;

/// <summary>Кошелёк и операции по нему.</summary>
public sealed class WalletController : ApiControllerBase
{
    private readonly IWalletService _wallets;

    /// <summary>Создаёт контроллер.</summary>
    public WalletController(IWalletService wallets) => _wallets = wallets;

    /// <summary>Пополняет кошелёк.</summary>
    /// <param name="request">Кому и сколько.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">Кошелёк пополнен.</response>
    /// <response code="400">Сумма не прошла проверку.</response>
    /// <response code="404">Пользователя нет.</response>
    [HttpPost("top-up")]
    public async Task<ActionResult<WalletDto>> TopUp(
        [FromBody] TopUpRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _wallets.TopUpAsync(request, cancellationToken));

    /// <summary>Состояние кошелька со сверкой баланса и суммы операций.</summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">Состояние кошелька.</response>
    /// <response code="404">Пользователя нет.</response>
    [HttpGet]
    public async Task<ActionResult<WalletDto>> Get(
        CancellationToken cancellationToken) =>
        Ok(await _wallets.GetAsync(cancellationToken));

    /// <summary>Выписка по кошельку, новые операции первыми.</summary>
    /// <param name="page">Номер страницы, с единицы.</param>
    /// <param name="pageSize">Размер страницы, не больше 50.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">Страница выписки.</response>
    [HttpGet("statement")]
    public async Task<ActionResult<PagedResult<WalletTransactionDto>>> GetStatement(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await _wallets.GetStatementAsync(
            page < 1 ? 1 : page,
            Math.Clamp(pageSize, 1, 50),
            cancellationToken));
}
