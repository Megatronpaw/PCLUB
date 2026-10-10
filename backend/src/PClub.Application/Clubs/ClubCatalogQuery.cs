namespace PClub.Application.Clubs
{
    /// <summary>
    /// Параметры запроса каталога клубов.
    /// </summary>
    public sealed record ClubCatalogQuery
    {
        /// <summary>Максимальный размер страницы. Больше сервер не отдаст.</summary>
        public const int MaxPageSize = 50;

        /// <summary>Размер страницы, если клиент не попросил свой.</summary>
        public const int DefaultPageSize = 20;

        /// <summary>Город. Сравнение точное.</summary>
        public string? City { get; init; }

        /// <summary>Подстрока в названии или адресе. Регистр не важен.</summary>
        public string? Search { get; init; }

        /// <summary>Минимальная ставка зоны, копейки. Ниже — отсечь.</summary>
        public long? MinPricePerHourCents { get; init; }

        /// <summary>Максимальная ставка зоны, копейки. Выше — отсечь.</summary>
        public long? MaxPricePerHourCents { get; init; }

        /// <summary>Порядок: name, price_asc, price_desc, seats_desc.</summary>
        public string? Sort { get; init; }

        /// <summary>Номер страницы, с единицы.</summary>
        public int Page { get; init; } = 1;

        /// <summary>Размер страницы.</summary>
        public int PageSize { get; init; } = DefaultPageSize;

        /// <summary>Номер страницы, приведённый к допустимому.</summary>
        public int NormalizedPage => Page < 1 ? 1 : Page;

        /// <summary>Начало интервала, на который нужны свободные места.</summary>
        public DateTimeOffset? From { get; init; }

        /// <summary>Окончание интервала. Не включается, как и везде.</summary>
        public DateTimeOffset? To { get; init; }

        /// <summary>Сколько мест нужно одновременно. По умолчанию одно.</summary>
        /// <remarks>
        /// Место — это машина, поэтому «трое друзей» означает «три свободных
        /// места в один и тот же интервал».
        /// </remarks>
        public int? SeatsNeeded { get; init; }

        /// <summary>Только клубы, открытые в текущий момент.</summary>
        public bool? OpenNow { get; init; }

        /// <summary>Задан ли интервал целиком.</summary>
        /// <remarks>
        /// Половина интервала бессмысленна: «свободно с 19:00» без «до» — это не
        /// вопрос. Поэтому фильтр включается только когда заданы оба конца.
        /// </remarks>
        public bool HasInterval => From is not null && To is not null;

        /// <summary>Сколько мест требовать, с приведением к разумному.</summary>
        public int NormalizedSeatsNeeded => SeatsNeeded is null or < 1 ? 1 : SeatsNeeded.Value;

        /// <summary>Размер страницы, приведённый к допустимому.</summary>
        public int NormalizedPageSize => PageSize switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => PageSize,
        };
    }
}
