using EventManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Services.Interfaces
{
    /// <summary>
    /// Интерфейс для работы с мероприятиями
    /// </summary>
    public interface IEventService
    {
        /// <summary>
        /// Создание мероприятия
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Guid CreateEvent(CreateUpdateEventDTO model);
        /// <summary>
        /// Обновление мероприятия
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        void UpdateEvent(Guid id,CreateUpdateEventDTO model);
        /// <summary>
        /// Получение всех мероприятий
        /// </summary>
        /// <returns></returns>
        List<EventDTO> GetAllEvents();
        /// <summary>
        /// Получение мероприятия по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        EventDTO? GetEventById(Guid id);
        /// <summary>
        /// Удаление мероприятия
        /// </summary>
        /// <param name="id"></param>
        void RemoveEvent(Guid id);

    }



}
