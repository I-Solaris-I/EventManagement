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
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<Guid> CreateEventAsync(CreateEventDTO model, CancellationToken ct = default);
        /// <summary>
        /// Обновление мероприятия
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="ct"></param>
        Task UpdateEventAsync(Guid id, UpdateEventDTO model, CancellationToken ct = default);
        /// <summary>
        /// Получение всех мероприятий
        /// </summary>
        /// <returns></returns>
        Task<List<EventDTO>> GetAllEventsAsync(CancellationToken ct = default);
        /// <summary>
        /// Получить эвенты с фильрацией и пагинацией
        /// </summary>
        /// <param name="title"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<PaginatedResultDto<EventDTO>> GetFilteredEventsAsync(EventFilterDTO model, CancellationToken ct = default);
        /// <summary>
        /// Получение мероприятия по id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<EventDTO> GetEventByIdAsync(Guid id, CancellationToken ct = default);
        /// <summary>
        /// Удаление мероприятия
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ct"></param>
        Task RemoveEventAsync(Guid id, CancellationToken ct = default);

    }



}
