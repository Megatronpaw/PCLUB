
namespace PClub.Domain.Exceptions
{
    /// <summary>
    /// Прав на это действие нет. Станет HTTP 403.
    /// </summary>
    public sealed class ForbiddenException :DomainException
    {
        ///<summary>Создаёт исключение "нельзя".</summary>
        public ForbiddenException(string message) : base(message)
        {
        }
    }
}
