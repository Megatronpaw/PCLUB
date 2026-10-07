using PClub.Domain.Enums;

namespace PClub.Application.Clubs
{
    /// <summary>
    /// Запрос смены состояния клуба.
    /// </summary>
    /// <param name="Status">Draft, Published или Hidden.</param>
    /// <remarks>
    /// Запись живёт в Application, а не в Api, чтобы валидатор нашёлся сам:
    /// AddValidatorsFromAssembly сканирует сборку Application, и если тип
    /// запроса лежит там же, связь устанавливается без явной регистрации
    /// в Program.cs — в отличие от ставки зоны.
    /// </remarks>
    public sealed record ChangeClubStatusRequest(ClubStatus Status);
}
