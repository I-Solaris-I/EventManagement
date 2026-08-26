using EventManagement.Models;
using EventManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Controllers
{
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
        public EventsController(IEventService eventService, ILogger<EventsController> logger)
        {
            _eventService = eventService;
            _logger = logger;
        }
        /// <summary>
        /// Получить мероприятия
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<IEnumerable<EventDTO>> GetEvents()
        {
            var events = _eventService.GetAllEvents();
            if (events.Count == 0)
            {
                return NoContent();
            }
            else return Ok(events);
        }
        /// <summary>
        /// Получить мероприятие по id
        /// </summary>
        /// <param name="id">id мероприятие</param>
        /// <returns></returns>
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
        [HttpPut("{id}")]
        public ActionResult UpdateEvent([FromBody][Required] CreateUpdateEventDTO model, [FromRoute][Required] Guid id)
        {
            _eventService.UpdateEvent(id, model);
            return NoContent();
        }
        /// <summary>
        /// Создать пероприятие 
        /// </summary>
        /// <param name="model">модель данных мероприятия для создания</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddEvent([FromBody][Required] CreateUpdateEventDTO model)
        {
            return CreatedAtAction(nameof(AddEvent), _eventService.CreateEvent(model));
        }
        /// <summary>
        /// Удалить пероприятие 
        /// </summary>
        /// <param name="id">id мероприятие</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public ActionResult RemoveEvent([FromRoute][Required] Guid id)
        {
            _eventService.RemoveEvent(id);
            return NoContent();
        }

    }
}
