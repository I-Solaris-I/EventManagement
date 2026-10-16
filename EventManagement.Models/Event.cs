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
    public class EventNotFoundedException : Exception
    {
        /// <summary>
        /// Идентификатор ненайденного мероприятия
        /// </summary>
        public Guid EventId { get; private set; }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор ненайденного мероприятия</param>
        public EventNotFoundedException(Guid Id) : base($"Мероприятие c Id={Id} не найдено")
        {
            EventId = Id;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор ненайденного мероприятия</param>
        /// <param name="message">Сообщение</param>
        public EventNotFoundedException(Guid Id, string? message) : base(message)
        {
            EventId = Id;
        }
        /// <summary>
        ///  Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор ненайденного мероприятия</param>
        /// <param name="message">Сообщение</param>
        /// <param name="innerException">Вложенное исключение</param>
        public EventNotFoundedException(Guid Id, string? message, Exception? innerException) : base(message, innerException)
        {
            EventId = Id;
        }

    }

    /// <summary>
    /// Исключение для отстутствующего мероприятия
    /// </summary>
    public class EventNoAvailableSeatsException : Exception
    {
        /// <summary>
        /// Идентификатор мероприятия с нехваткой мест
        /// </summary>
        public Guid EventId { get; private set; }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор мероприятия с нехваткой мест</param>
        public EventNoAvailableSeatsException(Guid Id) : base($"Нет доступных мест для мероприятия c Id={Id}")
        {
            EventId = Id;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор мероприятия с нехваткой мест</param>
        /// <param name="message">Сообщение</param>
        public EventNoAvailableSeatsException(Guid Id, string? message) : base(message)
        {
            EventId = Id;
        }
        /// <summary>
        ///  Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор мероприятия с нехваткой мест</param>
        /// <param name="message">Сообщение</param>
        /// <param name="innerException">Вложенное исключение</param>
        public EventNoAvailableSeatsException(Guid Id, string? message, Exception? innerException) : base(message, innerException)
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
        /// Общее количество мест на событии;
        /// </summary>
        public int TotalSeats { get; private set; }
        /// <summary>
        /// Текущее количество свободных мест на событии
        /// </summary>
        public int AvailableSeats { get; private set; }


        /// <summary>
        /// Контрутор Мероприятия
        /// </summary>
        /// <param name="id">Идентификатор</param>
        /// <param name="title">Наименование</param>
        /// <param name="startAt">Дата начала</param>
        /// <param name="endAt">Дата конца</param>
        /// <param name="totalSeats">Число мест для бронирования события</param>
        /// <param name="availableSeats">Доступное число мест для бронирования события</param>
        /// <param name="description">Описание</param>
        public Event(Guid id, string title, DateTime startAt, DateTime endAt, int totalSeats, int availableSeats, string? description = null) : this(title, startAt, endAt, totalSeats, description)
        {
            Id = id;
            if (availableSeats > totalSeats) throw new ArgumentException($"Достуное количество мест для бронирования не может превышать макимальное", paramName: nameof(availableSeats));
            if (availableSeats < 0) throw new ArgumentException($"Достуное количество мест для бронирования не может быть меньше 0", paramName: nameof(availableSeats));

            AvailableSeats = availableSeats;
        }

        /// <summary>
        /// Контрутор Мероприятия
        /// </summary>
        /// <param name="title">Наименование</param>
        /// <param name="startAt">Дата начала</param>
        /// <param name="endAt">Дата конца</param>
        /// <param name="totalSeats">Число мест для бронирования события</param>
        /// <param name="description">Описание</param>
        /// <exception cref="ArgumentException"></exception>
        public Event(string title, DateTime startAt, DateTime endAt, int totalSeats, string? description = null)
        {
            if (endAt <= startAt) throw new ArgumentException($"Дата окончания мероприятия {endAt} должна быть больше даты начала мероприяти {startAt}", paramName: nameof(endAt));
            if (totalSeats <= 0) throw new ArgumentException($"Количество мест для бронирования должно быть неотрицательным", paramName: nameof(totalSeats));

            Id = Guid.NewGuid();
            Title = title;
            Description = description;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = totalSeats;
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
        /// <summary>
        /// Попытка зарезервировать места 
        /// </summary>
        /// <param name="count">Количество резервируемых мест</param>
        /// <returns></returns>
        public bool TryReserveSeats(int count = 1)
        {
            if (AvailableSeats < count)
            {
                return false;
            }
            else
            {
                AvailableSeats -= count;
                return true;
            }
        }
        /// <summary>
        /// Освобожение мест на событии
        /// </summary>
        /// <param name="count">Количество освобождаемых мест</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void ReleaseSeats(int count = 1)
        {
            if (AvailableSeats + count > TotalSeats) throw new InvalidOperationException();
            else
            {
                AvailableSeats += count;
            }
        }


    }
}
