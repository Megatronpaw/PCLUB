using PClub.Domain.Enums;
using PClub.Domain.Exceptions;

namespace PClub.Domain.Entities
{
    ///<summary>Зона, к которой относится место.</summary>
    /// <summary>
    /// Одно место (машина) в клубе.
    /// </summary>
    public sealed class Seat
    {
        private Seat()
        {
        }

        /// <summary>
        /// Создаёт место. Метка обязательна.
        /// </summary>
        /// <param name="label">Метка на корпусе, например PC-01.</param>
        /// <param name="specs">Железо этого места; null означает «как в зоне».</param>
        /// <exception cref="ArgumentException">Метка пустая.</exception>
        public Seat(string label, string? specs = null)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new DomainValidationException("У места должна быть метка.");
            }

            Id = Guid.NewGuid();
            Label = label.Trim();
            Specs = specs?.Trim();
            Status = SeatStatus.Active;
        }
        /// <summary>
        /// Зона, к которой относится место.
        /// </summary>
        public Guid ZoneId { get; private set; }
        /// <summary>
        /// Идентификатор. Генерируется при создании.
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Метка на корпусе.
        /// </summary>
        public string Label { get; private set; } = null!;
        /// <summary>
        /// Железо места; null означает «как в зоне».
        /// </summary>

        public string? Specs { get; private set; }
        /// <summary>
        /// Текущее состояние.
        /// </summary>
        public SeatStatus Status { get; private set; }
        /// <summary>
        /// Отправляет место на обслуживание.
        /// </summary>
        public void SendToMaintenance() => Status = SeatStatus.Maintenance;
        /// <summary>
        /// Возвращает место в работу.
        /// </summary>
        public void ReturnToService() => Status = SeatStatus.Active;

        /// <summary>Зона, которой принадлежит место.</summary>
        /// <remarks>
        /// Нужна для Include(s => s.Zone).ThenInclude(z => z.Club): так ставка
        /// зоны и часы работы клуба приходят одним запросом, а не тремя.
        /// </remarks>
        public Zone Zone { get; private set; } = null!;
    }
}
