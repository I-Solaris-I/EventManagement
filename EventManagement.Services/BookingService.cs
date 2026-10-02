using EventManagement.Context.Interfaces;
using EventManagement.Models;
using EventManagement.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventManagement.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Booking> _repositoryBooking;
        private readonly IRepository<Event> _repositoryEvent;

        private readonly ILogger<BookingService> _logger;
        /// <summary>
        /// Сервис создания брони мероприятий
        /// </summary>
        /// <param name="repositoryBooking">репозиторий бронирований</param>
        /// <param name="repositoryEvent">репозиторий мероприятий</param>
        /// <param name="logger">логгер</param>
        public BookingService(IRepository<Booking> repositoryBooking, IRepository<Event> repositoryEvent, ILogger<BookingService> logger)
        {
            _logger = logger;
            _repositoryBooking = repositoryBooking;
            _repositoryEvent = repositoryEvent;
        }
        /// <summary>
        /// Создание новой брони мероприятия
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия</param>
        /// <param name="token">Токен отмены</param>
        /// <returns></returns>
        /// <exception cref="EventNotFoundedExсeption"></exception>
        /// <exception cref="BookingBusinessException"></exception>
        public async Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken token = default)
        {
            try
            {
                _logger.LogInformation($"Вызван метод {nameof(CreateBookingAsync)}");

                //Впоследствии репозиторий может (и должен) быть дописан с мспользованием асинхронных методов

                var evt = _repositoryEvent.GetById(eventId);
                if (evt == null) throw new EventNotFoundedExсeption(eventId);
                if (evt.StartAt <= DateTime.UtcNow) throw new BookingBusinessException("Невозможно создать бронь для мероприятия, которое началось");


                //Эмуляция долгой работы
                await Task.Delay(100, token);


                var booking = new Booking(eventId);
                _repositoryBooking.Create(booking);


                _logger.LogInformation($"Бронь с Id {booking.Id}, статус: {booking.Status.ToString()}");

                return BookingDTO.GetModel(booking);

            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                _logger.LogInformation($"Метод {nameof(CreateBookingAsync)} был отменён");
                throw;
            }

        }
        /// <summary>
        /// Получить мероприятие по идентификатору
        /// </summary>
        /// <param name="bookingId">Идентификатор мероприятия</param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="BookingNotFoundedException"></exception>
        public async Task<BookingDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken token = default)
        {
            try
            {
                _logger.LogInformation($"Вызван метод {nameof(GetBookingByIdAsync)}");

                //Впоследствии репозиторий может (и должен) быть дописан с мспользованием асинхронных методов

                //Эмуляция долгой работы
                await Task.Delay(100, token);
                 var booking = _repositoryBooking.GetById(bookingId);

                if (booking == null) throw new BookingNotFoundedException(bookingId);

                _logger.LogInformation($"Получена бронь с Id {booking.Id}, статус: {booking.Status.ToString()}");

                return BookingDTO.GetModel(booking);

            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                _logger.LogInformation($"Метод {nameof(GetBookingByIdAsync)} отменён");
                throw;
            }
        }
    }
}
