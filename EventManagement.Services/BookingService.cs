using EventManagement.Context.Interfaces;
using EventManagement.Models;
using EventManagement.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventManagement.Services
{
    public class BookingService : IBookingService
    {
        private readonly static SemaphoreSlim _bookingSemaphore = new(1, 1);
        private readonly IAsyncBookingRepository _repositoryBooking;
        private readonly IAsyncEventRepository _repositoryEvent;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<BookingService> _logger;
        /// <summary>
        /// Сервис создания брони мероприятий
        /// </summary>
        /// <param name="repositoryBooking">репозиторий бронирований</param>
        /// <param name="repositoryEvent">репозиторий мероприятий</param>
        /// <param name="logger">логгер</param>
        public BookingService(IAsyncBookingRepository repositoryBooking, IAsyncEventRepository repositoryEvent, IUnitOfWork uow, ILogger<BookingService> logger)
        {
            _logger = logger;
            _repositoryBooking = repositoryBooking;
            _repositoryEvent = repositoryEvent;
            _uow = uow;
        }
        /// <summary>
        /// Создание новой брони мероприятия
        /// </summary>
        /// <param name="eventId">Идентификатор мероприятия</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns></returns>
        /// <exception cref="EventNotFoundedException"></exception>
        /// <exception cref="EventNoAvailableSeatsException"></exception>
        /// <exception cref="BookingBusinessException"></exception>
        public async Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation($"Вызван метод {nameof(CreateBookingAsync)}");
                Booking booking;

                await _bookingSemaphore.WaitAsync(ct);
                try
                {
                    var evt = await _repositoryEvent.GetByIdAsync(eventId, ct);
                    if (evt == null) throw new EventNotFoundedException(eventId);
                    if (evt.StartAt <= DateTime.UtcNow) throw new BookingBusinessException("Невозможно создать бронь для мероприятия, которое началось");
                    if (!evt.TryReserveSeats())
                        throw new EventNoAvailableSeatsException(eventId);
                    //_repositoryEvent.Update(evt);
                    booking = new Booking(eventId);
                    _repositoryBooking.Add(booking);
                    await _uow.SaveChangesAsync(ct);
                }
                finally
                {
                    _bookingSemaphore.Release();
                }

                _logger.LogInformation($"Создана бронь с Id {booking.Id}, статус: {booking.Status.ToString()}");
                return BookingDTO.GetModel(booking);

            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogInformation($"Метод {nameof(CreateBookingAsync)} был отменён");
                throw;
            }

        }
        /// <summary>
        /// Получить мероприятие по идентификатору
        /// </summary>
        /// <param name="bookingId">Идентификатор брони</param>
        /// <param name="ct">Токен отмены</param>
        /// <returns></returns>
        /// <exception cref="BookingNotFoundedException"></exception>
        public async Task<BookingDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation($"Вызван метод {nameof(GetBookingByIdAsync)}");

                //Впоследствии репозиторий может (и должен) быть дописан с мспользованием асинхронных методов

                var booking = await _repositoryBooking.GetByIdAsync(bookingId, ct);

                if (booking == null) throw new BookingNotFoundedException(bookingId);

                _logger.LogInformation($"Получена бронь с Id {booking.Id}, статус: {booking.Status.ToString()}");

                return BookingDTO.GetModel(booking);

            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
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
        /// <param name="ct">Токен отмены</param>
        /// <returns></returns>
        /// <exception cref="BookingNotFoundedException"></exception>
        /// <exception cref="BookingBusinessException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<BookingDTO> ProcessPendingBookingAsync(Guid bookingId, CancellationToken ct = default)
        {
            _logger.LogInformation($"Вызван метод {nameof(ProcessPendingBookingAsync)}");
            await Task.Delay(TimeSpan.FromSeconds(_delayApiSec));
            await _bookingSemaphore.WaitAsync(ct);
            Booking? booking = null;
            Event? evt = null;
            try
            {

                booking = await _repositoryBooking.GetByIdAsync(bookingId, ct);
                if (booking == null) throw new BookingNotFoundedException(bookingId);


                if (booking.Status != BookingStatus.Pending) throw new BookingBusinessException($"Метод обрабатывает только бронирования в статусе {BookingStatus.Pending.ToString()}");

                evt = await _repositoryEvent.GetByIdAsync(booking.EventId, ct);
                if (evt == null)
                {
                    booking.Reject();
                    _logger.LogWarning("Мероприятие {EventId} не найдено, бронь {bookingId} отменена", booking?.EventId, booking?.Id);
                }
                else if (evt.StartAt <= DateTime.UtcNow)
                {
                    booking.Reject();
                    evt.ReleaseSeats();
                    _logger.LogWarning("Мероприятие {EventId} уже началось, бронь {bookingId} отменена", booking?.EventId, booking?.Id);

                }
                else booking.Confirm();
                await _uow.SaveChangesAsync(ct);

                return BookingDTO.GetModel(booking!);

            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                _logger.LogInformation($"Метод {nameof(ProcessPendingBookingAsync)} отменён");
                throw;
            }
            catch (BookingBusinessException)
            {
                throw;
            }
            catch (Exception)
            {
                if (booking != null)
                {
                    booking.Reject();
                }
                if (evt != null)
                {
                    evt.ReleaseSeats();
                }
                await _uow.SaveChangesAsync(ct);
                throw;
            }
            finally
            {
                _bookingSemaphore.Release();
            }
        }
    }
}
