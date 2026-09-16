using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Models
{
    /// <summary>
    /// Исключение для отстутствующего мероприятия
    /// </summary>
    public class EventNotFoundedExeption : Exception
    {
        /// <summary>
        /// Идентифкатор ненайденного мероприятия
        /// </summary>
        public Guid EventId { get; private set; }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор ненайденного мероприятия</param>
        public EventNotFoundedExeption(Guid Id) : base($"Мероприятие c Id={Id} не найдено")
        {
            EventId = Id;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор ненайденного мероприятия</param>
        /// <param name="message">Сообщение</param>
        public EventNotFoundedExeption(Guid Id, string? message) : base(message)
        {
            EventId = Id;
        }
        /// <summary>
        ///  Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор ненайденного мероприятия</param>
        /// <param name="message">Сообщение</param>
        /// <param name="innerException">Вложенное исключение</param>
        public EventNotFoundedExeption(Guid Id, string? message, Exception? innerException) : base(message, innerException)
        {
            EventId = Id;
        }

    }
    /// <summary>
    /// Мероприятие (Доменная модель)
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Идентификатор мероприятия
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        /// Наименование мероприятия
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Описание мероприятия
        /// </summary>
        public string? Description { get; private set; }
        /// <summary>
        /// Дата начала
        /// </summary>
        public DateTime StartAt { get; private set; }
        /// <summary>
        /// Дата окончания
        /// </summary>
        public DateTime EndAt { get; private set; }
        /// <summary>
        /// Контрутор Мероприятия
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="title">Наименование</param>
        /// <param name="startAt">Дата начала</param>
        /// <param name="endAt">Дата конца</param>
        /// <param name="description">Описание</param>
        public Event(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null) : this(title, startAt, endAt, description)
        {
            Id = id;
        }

        /// <summary>
        /// Контрутор Мероприятия
        /// </summary>
        /// <param name="title">Наименование</param>
        /// <param name="startAt">Дата начала</param>
        /// <param name="endAt">Дата конца</param>
        /// <param name="description">Описание</param>
        /// <exception cref="ArgumentException"></exception>
        public Event(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            if (endAt <= startAt) throw new ArgumentException($"Дата окончания мероприятия {endAt} должна быть больше даты начала мероприяти {startAt}");

            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }
        /// <summary>
        /// Обновления эвента
        /// </summary>
        /// <param name="title"></param>
        /// <param name="startAt"></param>
        /// <param name="endAt"></param>
        /// <param name="description"></param>
        public void UpdateEvent(string title, DateTime startAt, DateTime endAt, string? description = null)
        {
            if (endAt <= startAt) throw new ArgumentException($"Дата окончания мероприятия {endAt} должна быть больше даты начала мероприяти {startAt}");
            if (string.IsNullOrEmpty(title)) throw new ArgumentException($"Наименование мероприятия не должно быть пустым");

            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
        }
    }
}
