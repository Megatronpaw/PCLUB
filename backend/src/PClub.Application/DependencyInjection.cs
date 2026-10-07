using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PClub.Application.Clubs;
using PClub.Application.Zones;

namespace PClub.Application
{
    /// <summary>
    /// Регистрация всего, что умеет слой приложения.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Добавляет сервисы слоя приложения.
        /// </summary>
        /// <param name="services">Коллекция сервисов.</param>
        /// <returns>Её же — чтобы вызовы можно было ставить цепочкой.</returns>
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IClubService, ClubService>();
            services.AddScoped<IZoneService, ZoneService>();

            // Находит и регистрирует все AbstractValidator<T> этой сборки.
            // Добавил новый валидатор — он заработает без правки этого файла.
            services.AddValidatorsFromAssembly(
                typeof(DependencyInjection).Assembly,
                includeInternalTypes: true);

            return services;
        }
    }
}
