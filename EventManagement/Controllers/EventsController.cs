using EventManagement.Models;
using EventManagement.Services;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Controllers
{
    /// <summary>
    /// Контроллер мероприятий
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ILogger<EventsController> _logger;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="eventService">Репозиторий мероприятий</param>
        /// <param name="bookingService">Репозиторий бронкй</param>
        /// <param name="logger">логгер</param>
        public EventsController(IEventService eventService, IBookingService bookingService, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _bookingService = bookingService;
            _logger = logger;
        }
        /// <summary>
        /// Получить мероприятия с возможностью фильтрации и пагинации
        /// </summary>
        /// <param name="title">Наименование для фильтрации</param>
        /// <param name="from">Дата с для фильтрации</param>
        /// <param name="to">Дата по для фильтрации</param>
        /// <param name="page">Номер страницы для выдачи данных</param>
        /// <param name="pageSize">Кол-во элементов на странице</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns></returns>
        /// <response code="200">Возвращается список мероприятий</response>

        [HttpGet]
        public async Task<ActionResult<PaginatedResultDto<EventDTO>>> GetEvents(CancellationToken ct, [FromQuery] string? title, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return await _eventService.GetFilteredEventsAsync(new EventFilterDTO(title, from, to, page, pageSize), ct);
        }
        /// <summary>
        /// Получить мероприятие по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns></returns>
        /// <response code="200">Мероприятие получено</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<EventDTO>> GetEventById([FromRoute][Required] Guid id, CancellationToken ct)
        {
            return Ok(await _eventService.GetEventByIdAsync(id, ct));

        }
        /// <summary>
        /// Создать бронь мероприятия
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns></returns>
        /// <response code="202">Бронь создана, ожидает обработки</response>
        /// <response code="400">Мероприятие уже началось, бронирование невозможно</response>
        /// <response code="404">Мероприятие не найдено, бронирование невозможно</response>
        /// <response code="409">У мероприятия недостаточно свободных мест для бронирования</response>

        [HttpPost("{id}/book")]
        public async Task<ActionResult<BookingDTO>> CreateBooking([FromRoute] Guid id, CancellationToken ct)
        {
            var booking = await _bookingService.CreateBookingAsync(id, ct);
            return AcceptedAtAction(nameof(BookingsController.GetBooking), nameof(BookingsController).Replace("controller", "", StringComparison.InvariantCultureIgnoreCase), new { id = booking.Id }, booking);
        }


        /// <summary>
        /// Обновить мероприятие
        /// </summary>
        /// <param name="model">Модель данных мероприятия для изменения</param>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns></returns>
        /// <response code="204">Мероприятие обновлено</response>
        /// <response code="404">Мероприятие не найдено</response>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateEvent([FromBody][Required] UpdateEventDTO model, [FromRoute][Required] Guid id, CancellationToken ct)
        {
            await _eventService.UpdateEventAsync(id, model, ct);
            return NoContent();
        }
        /// <summary>
        /// Создать мероприятие 
        /// </summary>
        /// <param name="model">Модель данных мероприятия для создания</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns></returns>
        /// <response code="201">Мероприятие создано</response>
        [HttpPost]
        public async Task<ActionResult<Guid>> AddEvent([FromBody][Required] CreateEventDTO model, CancellationToken ct)
        {
            var eventId = await _eventService.CreateEventAsync(model, ct);
            return CreatedAtAction(nameof(GetEventById), new { id = eventId }, eventId);
        }
        /// <summary>
        /// Удалить мероприятие 
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns></returns>
        /// <response code="404">Мероприятие не найдено</response>
        /// <response code="204">Мероприятие удалено</response>
        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveEvent([FromRoute][Required] Guid id, CancellationToken ct)
        {
            await _eventService.RemoveEventAsync(id, ct);
            return NoContent();
        }

    }
}
