
namespace PClub.Domain.Enums
{
    /// <summary>
    /// Состояние клуба в каталоге.
    /// </summary>
    public enum ClubStatus
    {
        ///<summary>Черновик: владелец настраивает, в каталоге не виден.</summary>
        Draft = 0,
        ///<summary>Опубликован: виден в каталоге, принимает брони.</summary>
        Published = 1,
        ///<summary>Скрыт: убран из каталога, данные сохранены.</summary>
        Hidden =2,
    }   
}
