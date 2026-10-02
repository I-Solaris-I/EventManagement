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

        private IRepository<Event> _repository;
        private IValidator<EventFilterDTO> _validatorEF;
        private IValidator<CreateUpdateEventDTO> _validatorCUE;
        private readonly ILogger<EventService> _logger;

        /// <summary>
        /// Конструктор сервиса
        /// </summary>
        /// <param name="repository">Репозиторий мероприятий</param>
        /// <param name="logger">Логгер</param>
        /// <param name="validatorCUE">Валидатор DTO создания/обновления мероприятия</param>
        /// <param name="validatorEF">Валидатор фильтра выдачи мероприятий</param>
        public EventService(IRepository<Event> repository,
            ILogger<EventService> logger,
            IValidator<CreateUpdateEventDTO> validatorCUE,
            IValidator<EventFilterDTO> validatorEF)
            {
                _repository = repository;
                _validatorCUE = validatorCUE;
                _validatorEF = validatorEF;
                _logger = logger;
            }
        /// <summary>
        /// Создание мероприятия
        /// </summary>
        /// <param name="model">Модель мероприятия</param>
        /// <returns></returns>
        /// <exception cref="ValidationException"></exception>
        public Guid CreateEvent(CreateUpdateEventDTO model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            _logger.LogInformation($"Вызван метод {nameof(CreateEvent)}");
            var result = _validatorCUE.Validate(model);
            if (!result.IsValid) throw new ValidationException(result.Errors);
            var newEvent = new Event(model.Title, model.StartAt, model.EndAt, model.Description);
            _repository.Create(newEvent);
            _logger.LogInformation($"Мероприятия c {newEvent.Id} создано");
            return newEvent.Id;
        }
        /// <summary>
        /// Обновить мероприятие
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <param name="model">Модель обновления</param>
        /// <exception cref="EventNotFoundedExсeption"></exception>
        /// <exception cref="ValidationException"></exception>
        public void UpdateEvent(Guid id, CreateUpdateEventDTO model)
        {
            _logger.LogInformation($"Вызван метод {nameof(UpdateEvent)}");

            if (model == null) throw new ArgumentNullException(nameof(model));

            var result = _validatorCUE.Validate(model);
            if (!result.IsValid) throw new ValidationException(result.Errors);

            var evt = _repository.GetById(id);
            if (evt == null) throw new EventNotFoundedExсeption(id);

            evt.UpdateEvent(model.Title, model.StartAt, model.EndAt, model.Description);
            _repository.Update(evt);
            _logger.LogInformation($"Мероприятия c {id} обновлено");
        }
        /// <summary>
        /// Получить все мероприятия
        /// </summary>
        /// <returns></returns>
        public List<EventDTO> GetAllEvents()
        {
            _logger.LogInformation($"Вызван метод {nameof(GetAllEvents)}");
            var events = _repository.GetAll().Select(EventDTO.GetModel).ToList();
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
        public PaginatedResultDto<EventDTO> GetFilteredEvents(EventFilterDTO model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var result = _validatorEF.Validate(model);
            if (!result.IsValid) throw new ValidationException(result.Errors);

            DateTime _from = DateTime.MinValue;
            string _title = string.Empty;
            DateTime _to = DateTime.MinValue;
            bool hasTitleFiltration = false, hasFromFiltration = false, hasToFiltration = false;

            if (!string.IsNullOrEmpty(model.Title))
            {
                _title = model.Title;
                hasTitleFiltration = true;
            }
            if (model.From.HasValue)
            {
                _from = model.From!.Value.ToUniversalTime();
                hasFromFiltration = true;
            }
            if (model.To.HasValue)
            {
                _to = model.To!.Value.ToUniversalTime();
                hasToFiltration = true;
            }

            _logger.LogInformation($"Вызван метод {nameof(GetFilteredEvents)}");
            var query = _repository.GetAll();

            if (hasFromFiltration)
            {
                query = query.Where(a => a.StartAt >= _from);
            }
            if (hasToFiltration)
            {
                query = query.Where(a => a.EndAt <= _to);
            }
            if (hasTitleFiltration)
            {
                query = query.Where(a => a.Title.Contains(_title, StringComparison.InvariantCultureIgnoreCase));
            }

            var total = query.Count();

            query = query.Skip((model.Page - 1) * model.PageSize).Take(model.PageSize);

            var events = query.Select(EventDTO.GetModel).ToList();

            _logger.LogInformation($"Мероприятия выгружены");
            return new PaginatedResultDto<EventDTO>()
            {
                Items = events,
                PageSize = model.PageSize,
                Page = model.Page,
                Total = total,
            };
        }


        /// <summary>
        /// Получить мероприятие по ID
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <returns></returns>
        /// <exception cref="EventNotFoundedExсeption"></exception>
        public EventDTO GetEventById(Guid id)
        {
            _logger.LogInformation($"Вызван метод {nameof(GetEventById)}");

            var evt = _repository.GetById(id);
            if (evt == null) throw new EventNotFoundedExсeption(id);
            _logger.LogInformation($"Мероприятие с {evt.Id} получено");
            return EventDTO.GetModel(evt);
        }
        /// <summary>
        /// Удаление мероприятия
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <exception cref="EventNotFoundedExсeption"></exception>
        public void RemoveEvent(Guid id)
        {
            _logger.LogInformation($"Вызван метод {nameof(RemoveEvent)}");
            if (!_repository.IsExist(id)) throw new EventNotFoundedExсeption(id);
            _repository.Delete(id);
            _logger.LogInformation($"Мероприятие с {id} удалено");

        }


    }
}
