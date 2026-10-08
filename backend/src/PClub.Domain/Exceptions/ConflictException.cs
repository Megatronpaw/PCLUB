
namespace PClub.Domain.Exceptions
{
    /// <summary>
    /// Действие конфликтует с текущим состоянием. Станет HTTP 409.
    /// </summary>
    public sealed class ConflictException : DomainException
    {
        ///<summary>Создаёт исключение конфликта.</summary>
        public ConflictException(string message) : base(message) {
        }
    }
}
