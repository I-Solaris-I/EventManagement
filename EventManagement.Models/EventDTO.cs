using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Models
{
    /// <summary>
    /// Базовый интерфейс DTO
    /// </summary>
    public interface IEventDTO
    {
        /// <summary>
        /// Наименование мероприятия
        /// </summary>
        string Title { get; }
        /// <summary>
        /// Дата начала мероприятия
        /// </summary>
        DateTime StartAt { get; }
        /// <summary>
        /// Дата окончания мероприятия
        /// </summary>
        DateTime EndAt { get; }
    }
    /// <summary>
    /// DTO - модель для выдачи клиенту
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="Title"></param>
    /// <param name="StartAt"></param>
    /// <param name="EndAt"></param>
    /// <param name="AvailableSeats"></param>
    /// <param name="TotalSeats"></param>
    /// <param name="Description"></param>
    public record EventDTO(Guid Id, string Title, DateTime StartAt, DateTime EndAt, int AvailableSeats, int TotalSeats, string? Description = null) : IEventDTO
    {
        /// <summary>
        /// Получение модели представления из доменной модели
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static EventDTO GetModel(Event e)
        {
            return new EventDTO(e.Id, e.Title, e.StartAt, e.EndAt, e.AvailableSeats, e.TotalSeats, e.Description);
        }
    }
    /// <summary>
    /// DTO модель для создаваемого мероприятия
    /// </summary>
    /// <param name="Title"></param>
    /// <param name="StartAt"></param>
    /// <param name="EndAt"></param>
    ///  <param name="TotalSeats"></param>
    /// <param name="Description"></param>
    public record CreateEventDTO(string Title, DateTime StartAt, DateTime EndAt, int TotalSeats, string? Description = null) : IEventDTO;
    /// <summary>
    /// DTO модель для обновляемого мероприятия
    /// </summary>
    /// <param name="Title"></param>
    /// <param name="StartAt"></param>
    /// <param name="EndAt"></param>
    /// <param name="Description"></param>
    public record UpdateEventDTO(string Title, DateTime StartAt, DateTime EndAt, string? Description = null) : IEventDTO;

    /// <summary>
    /// Базовый класс валидации
    /// </summary>
    public abstract class EventBaseValidation<T> : AbstractValidator<T> where T : IEventDTO
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public EventBaseValidation()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .NotNull()
                .WithMessage("Название мероприятия не может быть пустым");
            RuleFor(x => x.StartAt)
                .NotEmpty()
               .WithMessage("Не указана дата начала мероприятия");
            RuleFor(x => x.EndAt)
               .NotEmpty()
               .WithMessage("Не указана дата окончания мероприятия");
            RuleFor(x => x.EndAt).GreaterThan(x => x.StartAt).WithMessage((x) => $"Дата окончания мероприятия {x.EndAt} должна быть больше даты начала мероприятия {x.StartAt}");
        }

    }
    /// <summary>
    /// Правила валидации
    /// </summary>
    public class CreateEventDTOValidation : EventBaseValidation<CreateEventDTO>
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public CreateEventDTOValidation()
        {
            RuleFor(x => x.TotalSeats).Must(x => x > 0).WithMessage("Количество мест для бронирования должно быть положительным");
        }

    }
    /// <summary>
    /// Правила валидации
    /// </summary>
    public class UpdateEventDTOValidation : EventBaseValidation<UpdateEventDTO>
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public UpdateEventDTOValidation()
        {
        }
    }
    /// <summary>
    /// Модель фильтрации мероприятий
    /// </summary>
    /// <param name="Title">Заголовок для фильтрации</param>
    /// <param name="From">Дата с для фильтрации</param>
    /// <param name="To">Дата по для фильтрации</param>
    /// <param name="Page">Номер страницы</param>
    /// <param name="PageSize">Кол-во элементов на странице</param>
    public record EventFilterDTO(string? Title = null, DateTime? From = null, DateTime? To = null, int Page = 1, int PageSize = 10);

    /// <summary>
    /// Валидатор для EventFilterDTO
    /// </summary>
    public class EventFilterDTOValidation : AbstractValidator<EventFilterDTO>
    {
        /// <summary>
        /// Конструктор валидатора
        /// </summary>
        public EventFilterDTOValidation()
        {
            RuleFor(x => x.Title).NotEmpty().When(x => x.Title != null).WithMessage("Заголовок для фильтрации не должен быть пустым");
            RuleFor(x => x.Page).GreaterThan(0).WithMessage("Номер страницы должен быть неотрицательным");
            RuleFor(x => x.PageSize).GreaterThanOrEqualTo(1).WithMessage("Число элементов на странице должно быть >= 1");
            RuleFor(x => x.PageSize).LessThanOrEqualTo(1000).WithMessage("Число элементов на странице не должно превышать 1000");
            RuleFor(x => x).Custom((x, context) =>
            {
                if (x.From.HasValue && x.To.HasValue)
                {
                    if (x.To.Value < x.From.Value)
                    {
                        context.AddFailure(
                            propertyName: nameof(x.To),
                            errorMessage: $"Дата окончания фильтрации {x.To} должна быть больше или равна дате начала {x.From}"
                        );
                    }
                }
            });
        }
    }
}
