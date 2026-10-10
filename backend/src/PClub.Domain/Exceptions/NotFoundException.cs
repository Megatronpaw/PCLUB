
namespace PClub.Domain.Exceptions
{
    /// <summary>
    ///Сущность не найдена. Станет HTTP 404.  
    /// </summary>
    public sealed class NotFoundException :DomainException
    {
        ///<summary>Создаёт исключение "такого нет"</summary>
        ///<param name="entity">Что искали, например "Seat".</param>
        ///<param name="key">По какому ключу искали.</param>
        public NotFoundException(string entity, object key)
            : base($"{entity} '{key}' не найден ")
        {
        }
    }
}
