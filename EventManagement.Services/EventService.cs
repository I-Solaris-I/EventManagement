using EventManagement.Context.Interfaces;
using EventManagement.Models;
using EventManagement.Services.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Services
{
    /// <summary>
    /// Сервич для работы с мероприятиями
    /// </summary>
    public class EventService : IEventService
    {

        private IAsyncEventRepository _repository;
        private IUnitOfWork _uow;

        private IValidator<EventFilterDTO> _validatorEF;
        private IValidator<CreateEventDTO> _validatorCE;
        private IValidator<UpdateEventDTO> _validatorUE;

        private readonly ILogger<EventService> _logger;

        /// <summary>
        /// Конструктор сервиса
        /// </summary>
        /// <param name="repository">Репозиторий мероприятий</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validatorCUE">Валидатор DTO создания/обновления мероприятия</param>
        /// <param name="validatorEF">Валидатор фильтра выдачи мероприятий</param>
        public EventService(IAsyncEventRepository repository,
            ILogger<EventService> logger,
            IValidator<CreateEventDTO> validatorCE,
            IValidator<UpdateEventDTO> validatorUE,
            IValidator<EventFilterDTO> validatorEF,
            IUnitOfWork uow)
        {
            _repository = repository;
            _validatorCE = validatorCE;
            _validatorUE = validatorUE;

            _validatorEF = validatorEF;
            _logger = logger;
            _uow = uow;
        }
        /// <summary>
        /// Создание мероприятия
        /// </summary>
        /// <param name="model">Модель мероприятия</param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public async Task<Guid> CreateEventAsync(CreateEventDTO model, CancellationToken ct = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            _logger.LogInformation($"Вызван метод {nameof(CreateEventAsync)}");
            var result = _validatorCE.Validate(model);
            if (!result.IsValid) throw new ValidationException(result.Errors);
            var newEvent = new Event(model.Title, model.StartAt, model.EndAt, model.TotalSeats, model.Description);
            _repository.Add(newEvent);
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation($"Мероприятия c {newEvent.Id} создано");
            return newEvent.Id;
        }
        /// <summary>
        /// Обновить мероприятие
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <param name="model">Модель обновления</param>
        /// <exception cref="EventNotFoundedException"></exception>
        /// <exception cref="ValidationException"></exception>
        public async Task UpdateEventAsync(Guid id, UpdateEventDTO model, CancellationToken ct = default)
        {
            _logger.LogInformation($"Вызван метод {nameof(UpdateEventAsync)}");

            if (model == null) throw new ArgumentNullException(nameof(model));

            var result = _validatorUE.Validate(model);
            if (!result.IsValid) throw new ValidationException(result.Errors);

            var evt = await _repository.GetByIdAsync(id);
            if (evt == null) throw new EventNotFoundedException(id);

            evt.UpdateEvent(model.Title, model.StartAt, model.EndAt, model.Description);
            // _repository.Update(evt);
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation($"Мероприятия c {id} обновлено");
        }
        /// <summary>
        /// Получить все мероприятия
        /// </summary>
        /// <returns></returns>
        public async Task<List<EventDTO>> GetAllEventsAsync(CancellationToken ct = default)
        {
            _logger.LogInformation($"Вызван метод {nameof(GetAllEventsAsync)}");
            var events = (await _repository.GetAllAsync()).Select(EventDTO.GetModel).ToList();
            _logger.LogInformation($"Мероприятия выгружены");
            return events;
        }


        /// <summary>
        /// Получить мероприятия по фильтру
        /// </summary>
        /// <param name="title">Фильтр наименования</param>
        /// <param name="from">Фильр даты с</param>
        /// <param name="to">Фильтр даты по</param>
        /// <param name="page">Номер страницы</param>
        /// <param name="pageSize">Размер страницы</param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public async Task<PaginatedResultDto<EventDTO>> GetFilteredEventsAsync(EventFilterDTO model, CancellationToken ct = default)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var result = _validatorEF.Validate(model);
            if (!result.IsValid) throw new ValidationException(result.Errors);


            var result1 = await _repository.GetAllPaginatedAsync(model, ct);
            _logger.LogInformation($"Мероприятия выгружены");
            return new PaginatedResultDto<EventDTO>()
            {
                Items = result1.Items.Select(EventDTO.GetModel).ToList(),
                PageSize = result1.PageSize,
                Page = result1.Page,
                Total = result1.Total,
            };
        }


        /// <summary>
        /// Получить мероприятие по ID
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <returns></returns>
        /// <exception cref="EventNotFoundedException"></exception>
        public async Task<EventDTO> GetEventByIdAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation($"Вызван метод {nameof(GetEventByIdAsync)}");

            var evt = await _repository.GetByIdAsync(id);
            if (evt == null) throw new EventNotFoundedException(id);
            _logger.LogInformation($"Мероприятие с {evt.Id} получено");
            return EventDTO.GetModel(evt);
        }
        /// <summary>
        /// Удаление мероприятия
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <exception cref="EventNotFoundedException"></exception>
        public async Task RemoveEventAsync(Guid id, CancellationToken ct = default)
        {
            _logger.LogInformation($"Вызван метод {nameof(RemoveEventAsync)}");
            if (!await _repository.IsExistAsync(id, ct)) throw new EventNotFoundedException(id);
            _repository.Remove(id);
            await _uow.SaveChangesAsync(ct);
            _logger.LogInformation($"Мероприятие с {id} удалено");

        }


    }
}
