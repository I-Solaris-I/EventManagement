using EventManagement.Models;
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
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="eventService"></param>
        /// <param name="logger"></param>
        public EventsController(IEventService eventService, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }
        /// <summary>
        /// Получить мероприятия
        /// </summary>
        /// <param name="title"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        /// <response code="200">Возвращается список мероприятий</response>

        [HttpGet]
        public ActionResult<PaginatedResultDto<EventDTO>> GetEvents([FromQuery] string? title, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            return _eventService.GetFilteredEvents(new EventFilterDTO(title, from, to, page, pageSize));
        }
        /// <summary>
        /// Получить мероприятие по id
        /// </summary>
        /// <param name="id">id мероприятие</param>
        /// <returns></returns>
        /// <response code="200">Мероприятие получено</response>

        [HttpGet("{id}")]
        public ActionResult<EventDTO> GetEventById([FromRoute][Required] Guid id)
        {
            return Ok(_eventService.GetEventById(id));

        }
        /// <summary>
        /// Обновить мероприятие
        /// </summary>
        /// <param name="model">модель данных мероприятия для изменения</param>
        /// <param name="id">id мероприятие</param>
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
        /// <param name="model">модель данных мероприятия для создания</param>
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
        /// <param name="id">id мероприятия</param>
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
