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
        /// <returns></returns>
        /// <response code="200">Возвращается список мероприятий</response>

        [HttpGet]
        public ActionResult<PaginatedResultDto<EventDTO>> GetEvents([FromQuery] string? title, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return _eventService.GetFilteredEvents(new EventFilterDTO(title, from, to, page, pageSize));
        }
        /// <summary>
        /// Получить мероприятие по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <returns></returns>
        /// <response code="200">Мероприятие получено</response>

        [HttpGet("{id}")]
        public ActionResult<EventDTO> GetEventById([FromRoute][Required] Guid id)
        {
            return Ok(_eventService.GetEventById(id));

        }
        /// <summary>
        /// Создать бронь мероприятия
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <returns></returns>
        /// <response code="200">Бронь создана, ожидает обработки</response>
        [HttpPost("{id}/book")]
        public async Task<ActionResult<BookingDTO>> CreateBooking([FromRoute] Guid id)
        {
            var booking = await _bookingService.CreateBookingAsync(id);
            return AcceptedAtAction(nameof(BookingsController.GetBooking), nameof(BookingsController).Replace("controller", "", StringComparison.InvariantCultureIgnoreCase),new {id=booking.Id}, booking);
        }


        /// <summary>
        /// Обновить мероприятие
        /// </summary>
        /// <param name="model">Модель данных мероприятия для изменения</param>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <returns></returns>
        /// <response code="204">Мероприятие обновлено</response>
        /// <response code="404">Мероприятие не найдено</response>
        [HttpPut("{id}")]
        public ActionResult UpdateEvent([FromBody][Required] CreateUpdateEventDTO model, [FromRoute][Required] Guid id)
        {
            _eventService.UpdateEvent(id, model);
            return NoContent();
        }
        /// <summary>
        /// Создать мероприятие 
        /// </summary>
        /// <param name="model">Модель данных мероприятия для создания</param>
        /// <returns></returns>
        /// <response code="201">Мероприятие создано</response>
        [HttpPost]
        public ActionResult<Guid> AddEvent([FromBody][Required] CreateUpdateEventDTO model)
        {
            var eventId = _eventService.CreateEvent(model);
            return CreatedAtAction(nameof(GetEventById), new { id = eventId }, eventId);
        }
        /// <summary>
        /// Удалить мероприятие 
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <returns></returns>
        /// <response code="404">Мероприятие не найдено</response>
        /// <response code="204">Мероприятие удалено</response>

        [HttpDelete("{id}")]
        public ActionResult RemoveEvent([FromRoute][Required] Guid id)
        {
            _eventService.RemoveEvent(id);
            return NoContent();
        }

    }
}
