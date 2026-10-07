using Microsoft.AspNetCore.Mvc;
namespace PClub.Api.Controllers
{
    /// <summary>
    /// Общий базовый класс контроллеров: задаёт шаблон маршрута в одном месте, чтобы новый контроллер не смог случайно оказаться вне /api/v1.
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
   public abstract class ApiControllerBase: ControllerBase
    {
    }
}
