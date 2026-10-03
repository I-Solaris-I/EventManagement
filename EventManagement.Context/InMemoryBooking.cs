using Bogus;
using EventManagement.Context.Interfaces;
using EventManagement.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Context
{
    /// <summary>
    /// Репозиторий бронирований
    /// </summary>
    public class InMemoryBooking : IBookingRepository
    {
        private const int randomSetConfirmed = 100;
        private const int randomSetProcessed = 55;
        private const int fakerSeed = 8675309;
        private const int generateCount = 50;
        private readonly Lock _lock;
        private readonly IRepository<Event> _eventRepository;
        private List<Booking> _bookings;
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="eventRepository">репозиторий мероприятий</param>
        public InMemoryBooking(IRepository<Event> eventRepository)
        {

            _lock = new();
            _eventRepository = eventRepository;
            var eventData = _eventRepository.GetAll().Where(a => a.StartAt > DateTime.UtcNow).ToArray();
            if (eventData.Length == 0) { _bookings = new(); return; }
            var test_data = new Faker<Booking>().CustomInstantiator((f) =>
            {

                var randomInt = f.Random.Int(50, 150);

                var randomEventIndex = f.Random.Int(0, eventData.Length - 1);

                var eventId = eventData[randomEventIndex].Id;
                var createdAt = DateTime.UtcNow.AddMinutes(-f.Random.Int(1, 5));
                var status = BookingStatus.Pending;

                var processedAt = randomInt > randomSetProcessed ? createdAt.AddMinutes(f.Random.Int(1, 5)) : (DateTime?)default;
                if (processedAt.HasValue)
                {
                    status = randomInt > randomSetConfirmed ? BookingStatus.Confirmed : BookingStatus.Rejected;
                }
                return new Booking(f.Random.Guid(), status, eventId, createdAt, processedAt);
            });
            _bookings = test_data.UseSeed(fakerSeed).Generate(generateCount).OrderByDescending(u => u.ProcessedAt).ToList();

        }
        /// <summary>
        /// Создание брони
        /// </summary>
        /// <param name="data">данные для создания брони</param>
        public void Create(Booking data)
        {
            using (_lock.EnterScope())
            {
                _bookings.Add(data);
            }

        }
        /// <summary>
        /// Обновление брони
        /// </summary>
        /// <param name="data">Данные обновлённой брони</param>
        public void Update(Booking data)
        {
            using (_lock.EnterScope())
            {
                var eventItem = _bookings.FirstOrDefault(e => e.Id == data.Id);
                if (eventItem != null)
                {
                    _bookings.Remove(eventItem);
                    _bookings.Add(data);
                }
            }
        }
        /// <summary>
        /// Проверить, существует ли бронь без её возврата
        /// </summary>
        /// <param name="id">Идентификатор брони</param>
        /// <returns></returns>
        public bool IsExist(Guid id)
        {
            using (_lock.EnterScope())
            {
                return _bookings.Any(a => a.Id == id);
            }
        }
        /// <summary>
        /// Получить все бронирования
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Booking> GetAll()
        {
            using (_lock.EnterScope())
            {
                return _bookings.ToList();
            }
        }

        /// <summary>
        /// Получить бронь по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор брони</param>
        /// <returns></returns>
        public Booking? GetById(Guid id)
        {
            using (_lock.EnterScope())
            {
               var booking=_bookings.FirstOrDefault(a => a.Id == id);
                if (booking != null)
                {
                    return new Booking(booking.Id, booking.Status, booking.EventId, booking.CreatedAt, booking.ProcessedAt);
                }
                else return null;
            }

        }
        /// <summary>
        /// Удаление брони
        /// </summary>
        /// <param name="id">Идентификатор брони</param>
        public void Delete(Guid id)
        {
            using (_lock.EnterScope())
            {
                var bookingItem = _bookings.FirstOrDefault(e => e.Id == id);
                if (bookingItem != null)
                {
                    _bookings.Remove(bookingItem);
                }
            }
        }
        /// <summary>
        /// Подтверждение брони
        /// </summary>
        /// <param name="id"></param>
        public Booking? Confirm(Guid id)
        {
            using (_lock.EnterScope())
            {
                var bookingItem = _bookings.FirstOrDefault(e => e.Id == id);
                if (bookingItem != null && bookingItem.Status == BookingStatus.Pending)
                {
                    bookingItem.Confirm();
                    return new Booking(bookingItem.Id, bookingItem.Status, bookingItem.EventId, bookingItem.CreatedAt, bookingItem.ProcessedAt);

                }
                else return null;
            }
        }
        /// <summary>
        /// Отмена брони
        /// </summary>
        /// <param name="id"></param>
        public Booking? Reject(Guid id)
        {
            using (_lock.EnterScope())
            {
                var bookingItem = _bookings.FirstOrDefault(e => e.Id == id);
                if (bookingItem != null && bookingItem.Status == BookingStatus.Pending)
                {
                    bookingItem.Reject();
                    return new Booking(bookingItem.Id, bookingItem.Status, bookingItem.EventId, bookingItem.CreatedAt, bookingItem.ProcessedAt);
                }
                else return null;
            }
        }
    }
}
