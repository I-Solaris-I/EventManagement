using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Models
{
    /// <summary>
    /// Исключение для отстутствующей брони мероприятия
    /// </summary>
    public class BookingNotFoundedException : Exception
    {
        /// <summary>
        /// Идентификатор ненайденной брони мероприятия
        /// </summary>
        public Guid BookingId { get; private set; }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор ненайденной брони мероприятия</param>
        public BookingNotFoundedException(Guid Id) : base($"Бронь с Id={Id} не найдена")
        {
            BookingId = Id;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор ненайденной брони мероприятия</param>
        /// <param name="message">Сообщение</param>
        public BookingNotFoundedException(Guid Id, string? message) : base(message)
        {
            BookingId = Id;
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="Id">Идентификатор ненайденной брони мероприятия</param>
        /// <param name="message">Сообщение</param>
        /// <param name="innerException">Вложенное исключение</param>
        public BookingNotFoundedException(Guid Id, string? message, Exception? innerException) : base(message, innerException)
        {
            BookingId = Id;
        }
    }

    /// <summary>
    /// Бизнес-исключение для брони
    /// </summary>
    public class BookingBusinessException : Exception
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public BookingBusinessException() : base("Неизвестная ошибка")
        {
        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="message">Сообщение</param>
        public BookingBusinessException(string? message) : base(message)
        {

        }
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="innerException">Вложенное исключение</param>
        public BookingBusinessException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
    /// <summary>
    /// Бронь мероприятия
    /// </summary>
    public class Booking
    {
        /// <summary>
        /// Уникальный идентификатор брони
        /// </summary>
        public Guid Id { get; private set; }
        /// <summary>
        ///  Идентификатор события, к которому относится бронь
        /// </summary>
        public Guid EventId { get; private set; }
        /// <summary>
        /// Дата и время создания брони
        /// </summary>
        public DateTime CreatedAt { get; private set; }
        /// <summary>
        ///  Текущий статус брони
        /// </summary>
        public BookingStatus Status { get; private set; }
        /// <summary>
        /// Дата и время обработки
        /// </summary>
        public DateTime? ProcessedAt { get; private set; }

        /// <summary>
        /// Подтвержение брони
        /// </summary>
        public void Confirm()
        {
            Status = BookingStatus.Confirmed;
            ProcessedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Отклонение брони
        /// </summary>
        public void Reject()
        {
            Status = BookingStatus.Rejected;
            ProcessedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Создание брони
        /// </summary>
        /// <param name="eventId"></param>
        public Booking(Guid eventId)
        {
            Id = Guid.NewGuid();
            EventId = eventId;
            Status = BookingStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }
        /// <summary>
        /// Конструктор для генерации
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="eventId"></param>
        /// <param name="createdAt"></param>
        /// <param name="processedAt"></param>
        /// <exception cref="ArgumentException"></exception>
        public Booking(Guid id, BookingStatus status, Guid eventId, DateTime createdAt, DateTime? processedAt = null)
        {
            if (processedAt.HasValue)
            {
                if (status == BookingStatus.Pending)
                    throw new ArgumentException($"Бронь с датой обработки не может быть в статусе {BookingStatus.Pending.ToString()}", paramName: nameof(status));
            }


            if (processedAt.HasValue && processedAt.Value < createdAt)
            {
                throw new ArgumentException($"Дата окончания обработки не может быть меньше даты создания", paramName: nameof(processedAt));
            }
            Id = id;
            EventId = eventId;
            Status = status;
            CreatedAt = createdAt;
            ProcessedAt = processedAt;
        }
    }


    /// <summary>
    /// Статус брони
    /// </summary>
    public enum BookingStatus
    {
        /// <summary>
        /// Бронь создана, ожидает обработки
        /// </summary>
        Pending,
        /// <summary>
        /// Бронь подтверждена
        /// </summary>
        Confirmed,
        /// <summary>
        /// Бронь отклонена
        /// </summary>
        Rejected

    }
}
