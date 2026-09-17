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
        /// <param name="repository"></param>
        /// <param name="validator"></param>
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
        /// <exception cref="EventNotFoundedExeption"></exception>
        /// <exception cref="ValidationException"></exception>
        public void UpdateEvent(Guid id, CreateUpdateEventDTO model)
        {
            _logger.LogInformation($"Вызван метод {nameof(UpdateEvent)}");

            if (!_repository.IsExist(id)) throw new EventNotFoundedExeption(id);
            if (model == null) throw new ArgumentNullException(nameof(model));

            var result = _validatorCUE.Validate(model);
            if (!result.IsValid) throw new ValidationException(result.Errors);

            if (!_repository.IsExist(id)) throw new EventNotFoundedExeption(id);

            var _event = _repository.GetById(id)!;
            _event.UpdateEvent(model.Title, model.StartAt, model.EndAt, model.Description);
            _logger.LogInformation($"Мероприятия c {id} обновлено");
            _repository.Update(_event);



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
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
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
        /// <exception cref="EventNotFoundedExeption"></exception>
        public EventDTO? GetEventById(Guid id)
        {
            _logger.LogInformation($"Вызван метод {nameof(GetEventById)}");

            if (!_repository.IsExist(id)) throw new EventNotFoundedExeption(id);

            var _event = EventDTO.GetModel(_repository.GetById(id)!);
            _logger.LogInformation($"Мероприятие с {id} получено");
            return _event;
        }
        /// <summary>
        /// Удаление мероприятие
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <exception cref="EventNotFoundedExeption"></exception>
        public void RemoveEvent(Guid id)
        {
            _logger.LogInformation($"Вызван метод {nameof(RemoveEvent)}");
            if (!_repository.IsExist(id)) throw new EventNotFoundedExeption(id);
            _repository.Delete(id);
            _logger.LogInformation($"Мероприятие с {id} удалено");

        }


    }
}
