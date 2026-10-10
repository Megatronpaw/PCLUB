using Microsoft.AspNetCore.Mvc;
using PClub.Application.Clubs;
using PClub.Application.Common;

namespace PClub.Api.Controllers
{
    /// <summary>
    /// Клубы и их структура.
    /// </summary>
    public sealed class ClubsController : ApiControllerBase
    {
        private readonly IClubService _clubs;

        /// <summary>
        /// Создаёт контроллер.
        /// </summary>
        public ClubsController(IClubService clubs) => _clubs = clubs;

        /// <summary>Возвращает все клубы со зонами и местами.</summary>
        /// <param name="query">Параметры выборки из строки запроса.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <response code="200">Список клубов.</response>
        [HttpGet]
        public async Task<ActionResult<PagedResult<ClubListItemDto>>> Get(
            [FromQuery] ClubCatalogQuery query,
            CancellationToken cancellationToken) =>
            Ok(await _clubs.SearchAsync(query, cancellationToken));

        /// <summary>Возвращает один клуб.</summary>
        /// <param name="id">Идентификатор клуба.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <response code="200">Клуб найден.</response>
        /// <response code="404">Клуба с таким идентификатором нет.</response>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClubDto>> GetById(
            Guid id, CancellationToken cancellationToken) =>
            Ok(await _clubs.GetByIdAsync(id, cancellationToken));

        /// <summary>
        /// Меняет состояние клуба в каталоге.
        /// </summary>
        /// <param name="id">Идентификатор клуба.</param>
        /// <param name="request">Новое состояние.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <response code="200">Состояние изменено.</response>
        /// <response code="404">Клуба нет.</response>
        /// <response code="409">Публикация клуба без активных мест.</response>
        [HttpPut("{id:guid}/status")]
        public async Task<ActionResult<ClubDto>> ChangeStatus(
            Guid id,
            [FromBody] ChangeClubStatusRequest request,
            CancellationToken cancellationToken) =>
            Ok(await _clubs.ChangeStatusAsync(id, request.Status, cancellationToken));
    }
}