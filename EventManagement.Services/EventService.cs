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
        private IValidator<CreateUpdateEventDTO> _validator;
        private readonly ILogger<EventService> _logger;

        /// <summary>
        /// Конструктор сервиса
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="validator"></param>
        public EventService(IRepository<Event> repository, IValidator<CreateUpdateEventDTO> validator, ILogger<EventService> logger)
        {
            _repository = repository;
            _validator = validator;
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
            _logger.LogInformation($"Вызван метод {nameof(CreateEvent)}");
            var result = _validator.Validate(model);
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

            var result = _validator.Validate(model);
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
