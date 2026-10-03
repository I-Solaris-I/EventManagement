using EventManagement.Models;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers
{
    /// <summary>
    /// Контроллер бронирования мероприятий
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly ILogger<BookingsController> _logger;
        private readonly IBookingService _bookingService;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="bookingService">Сервис бронирования мероприятий</param>
        /// <param name="logger">Логгер</param>
        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }
        /// <summary>
        /// Получить информацию о бронировании
        /// </summary>
        /// <param name="id">Идентификатор брони</param>
        /// <param name="ct">Токен отмены </param>
        /// <returns></returns>
        /// <response code="200">Получена информация о бронировании</response>
        /// <response code="404">Бронь не найдена</response>

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDTO>> GetBooking([FromRoute] Guid id, CancellationToken ct)
        {
            return Ok(await _bookingService.GetBookingByIdAsync(id, ct));
        }
    }
}
