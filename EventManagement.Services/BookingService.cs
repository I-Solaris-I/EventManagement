using EventManagement.Context.Interfaces;
using EventManagement.Models;
using EventManagement.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventManagement.Services
{
    public class BookingService : IBookingService
    {
        private readonly static object _bookingLock = new object();
        private readonly static SemaphoreSlim _bookingSemaphore = new(1, 1);

        private readonly IBookingRepository _repositoryBooking;
        private readonly IRepository<Event> _repositoryEvent;
        private readonly ILogger<BookingService> _logger;
        /// <summary>
        /// Сервис создания брони мероприятий
        /// </summary>
        /// <param name="repositoryBooking">репозиторий бронирований</param>
        /// <param name="repositoryEvent">репозиторий мероприятий</param>
        /// <param name="logger">логгер</param>
        public BookingService(IBookingRepository repositoryBooking, IRepository<Event> repositoryEvent, ILogger<BookingService> logger)
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
        /// <exception cref="EventNotFoundedException"></exception>
        /// <exception cref="EventNoAvailableSeatsException"></exception>
        /// <exception cref="BookingBusinessException"></exception>
        public async Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken token = default)
        {
            try
            {
                _logger.LogInformation($"Вызван метод {nameof(CreateBookingAsync)}");
                Booking booking;

                lock (_bookingLock)
                {
                    var evt = _repositoryEvent.GetById(eventId);
                    if (evt == null) throw new EventNotFoundedException(eventId);
                    if (evt.StartAt <= DateTime.UtcNow) throw new BookingBusinessException("Невозможно создать бронь для мероприятия, которое началось");
                    if (!evt.TryReserveSeats())
                        throw new EventNoAvailableSeatsException(eventId);
                    _repositoryEvent.Update(evt);
                    booking = new Booking(eventId);
                    _repositoryBooking.Create(booking);

                }

                _logger.LogInformation($"Создана бронь с Id {booking.Id}, статус: {booking.Status.ToString()}");
                return await Task.FromResult(BookingDTO.GetModel(booking));

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
        /// <param name="bookingId">Идентификатор брони</param>
        /// <param name="token">Токен отмены</param>
        /// <returns></returns>
        /// <exception cref="BookingNotFoundedException"></exception>
        public async Task<BookingDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken token = default)
        {
            try
            {
                _logger.LogInformation($"Вызван метод {nameof(GetBookingByIdAsync)}");

                //Впоследствии репозиторий может (и должен) быть дописан с мспользованием асинхронных методов

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
        private const int _delayApiSec = 2;

        /// <summary>
        /// Обработка созданной брони
        /// </summary>
        /// <param name="bookingId">Идентификатор брони</param>
        /// <param name="token">Токен отмены</param>
        /// <returns></returns>
        /// <exception cref="BookingNotFoundedException"></exception>
        /// <exception cref="BookingBusinessException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<BookingDTO> ProcessPendingBookingAsync(Guid bookingId, CancellationToken token = default)
        {
            _logger.LogInformation($"Вызван метод {nameof(ProcessPendingBookingAsync)}");
            await Task.Delay(TimeSpan.FromSeconds(_delayApiSec));
            await _bookingSemaphore.WaitAsync(token);
            try
            {

                var booking = _repositoryBooking.GetById(bookingId);
                if (booking == null) throw new BookingNotFoundedException(bookingId);


                if (booking.Status != BookingStatus.Pending) throw new BookingBusinessException($"Метод обрабатывает только бронирования в статусе {BookingStatus.Pending.ToString()}");

                var evt = _repositoryEvent.GetById(booking.EventId);
                if (evt == null)
                {
                    booking = _repositoryBooking.Reject(booking.Id);
                    _logger.LogWarning("Мероприятие {EventId} не найдено, бронь {bookingId} отменена", booking?.EventId, booking?.Id);
                }
                else if (evt.StartAt <= DateTime.UtcNow)
                {
                    booking = _repositoryBooking.Reject(booking.Id);
                    evt.ReleaseSeats();
                    _repositoryEvent.Update(evt);
                    _logger.LogWarning("Мероприятие {EventId} уже началось, бронь {bookingId} отменена", booking?.EventId, booking?.Id);

                }
                else booking = _repositoryBooking.Confirm(booking.Id);

                if (booking == null) throw new InvalidOperationException("Не получена бронь с изменённым статусом");

                return BookingDTO.GetModel(booking);

            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                _logger.LogInformation($"Метод {nameof(ProcessPendingBookingAsync)} отменён");
                throw;
            }
            finally
            {
                _bookingSemaphore.Release();
            }
        }
    }
}
