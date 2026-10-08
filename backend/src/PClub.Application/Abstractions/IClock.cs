
namespace PClub.Application.Abstractions
{
    /// <summary>
    /// Текущее время как зависимость, а не как обращение к статике.
    /// </summary>
    /// <remarks>
    /// Всё, что зависит от «сейчас», иначе нельзя протестировать, не переводя
    /// часы на машине. С этим интерфейсом тест просто говорит «пусть сейчас
    /// 18:00 первого июня» и проверяет поведение.
    /// </remarks>
    public interface IClock
    {
        /// <summary>
        /// Текущий момент в UTC.
        /// </summary>
        DateTimeOffset UtcNow { get; }
    }
}
