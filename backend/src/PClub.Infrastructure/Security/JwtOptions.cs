namespace PClub.Infrastructure.Security;

/// <summary>Параметры выпуска и проверки токенов.</summary>
public sealed class JwtOptions
{
    /// <summary>Имя раздела в конфигурации.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Минимальная длина ключа подписи в символах.</summary>
    public const int MinSigningKeyLength = 32;

    /// <summary>Кто выпустил токен.</summary>
    public string Issuer { get; init; } = "pclub";

    /// <summary>Для кого выпущен.</summary>
    public string Audience { get; init; } = "pclub-api";

    /// <summary>Ключ подписи. Только из окружения или локального файла.</summary>
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>Сколько живёт access-токен.</summary>
    public TimeSpan AccessTokenLifetime { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>Проверяет, что настройки годны к работе.</summary>
    /// <exception cref="InvalidOperationException">Настройки не заданы или слабы.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SigningKey))
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey не задан. Положи его в .env или " +
                "appsettings.Development.json — см. .env.example.");
        }

        if (SigningKey.Length < MinSigningKeyLength)
        {
            throw new InvalidOperationException(
                $"Jwt:SigningKey короче {MinSigningKeyLength} символов. " +
                "Сгенерируй: openssl rand -base64 48");
        }

        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("Jwt:Issuer и Jwt:Audience обязательны.");

        if (AccessTokenLifetime <= TimeSpan.Zero ||
            AccessTokenLifetime > TimeSpan.FromHours(1))
        {
            throw new InvalidOperationException(
                "Jwt:AccessTokenLifetime должен быть от нуля до часа.");
        }
    }
}
