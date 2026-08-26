using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Models
{
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

        public Event(Guid id, string title, DateTime startAt, DateTime endAt, string? description = null) : this(title, startAt, endAt, description)
        {
            Id = id;
        }

        /// <summary>
        /// Контрутор Мероприятия
        /// </summary>
        /// <param name="title">Наименование</param>
        /// <param name="startAt">Дата начала</param>
        /// <param name="endAt"></param>
        /// <param name="description"></param>
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


    /// <summary>
    /// Модель для выдачи клиенту
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Title"></param>
    /// <param name="StartAt"></param>
    /// <param name="EndAt"></param>
    /// <param name="Description"></param>
    public record EventDTO(Guid Id, string Title, DateTime StartAt, DateTime EndAt, string? Description = null)
    {
        /// <summary>
        /// Получение модели представления из доменной модели
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static EventDTO GetModel(Event e)
        {
            return new EventDTO(e.Id, e.Title, e.StartAt, e.EndAt, e.Description);
        }
    }


    /// <summary>
    /// DTO модель для создаваемого мероприятия
    /// </summary>
    /// <param name="Title"></param>
    /// <param name="StartAt"></param>
    /// <param name="EndAt"></param>
    /// <param name="Description"></param>
    public record CreateUpdateEventDTO(string Title, DateTime StartAt, DateTime EndAt, string? Description = null);
    /// <summary>
    /// Правила валидации
    /// </summary>
    public class CreateUpdateEventDTOValidation : AbstractValidator<CreateUpdateEventDTO>
    {
        /// <summary>
        /// Конмтруктор
        /// </summary>
        public CreateUpdateEventDTOValidation()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .NotNull()
                .WithMessage("Название мероприятия не может быть пустым");
            RuleFor(x => x.StartAt)
               .NotNull()
               .WithMessage("Не указана дата начала мероприятия");
            RuleFor(x => x.EndAt)
               .NotNull()
               .WithMessage("Не указана дата окончания мероприятия");
            RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt)
                .WithMessage((x) => $"Дата окончания мероприятия {x.EndAt} должна быть больше даты начала мероприяти {x.StartAt}");
        }

    }
}
