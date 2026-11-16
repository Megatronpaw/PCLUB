namespace PClub.Domain.Enums;

/// <summary>Роль пользователя.</summary>
public enum UserRole
{
    /// <summary>Обычный клиент: бронирует и платит.</summary>
    User = 1,

    /// <summary>Владелец клуба: управляет своими клубами.</summary>
    Owner = 2,

    /// <summary>Администратор сервиса.</summary>
    Admin = 3
}
