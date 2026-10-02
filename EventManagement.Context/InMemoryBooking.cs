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
    public class InMemoryBooking : IRepository<Booking>
    {
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
            _bookings = new();
            _eventRepository = eventRepository;
            Randomizer.Seed = new Random(8675309);

            var eventData = _eventRepository.GetAll().ToArray();
            var test_data = new Faker<Booking>().CustomInstantiator((f) =>
            {

                var createdAt = f.Date.Between(DateTime.Now.AddDays(-365), DateTime.Now.AddDays(365));
                var random1 = f.Random.Int(50, 150);

                var random2 = f.Random.Int(0, eventData.Length-1);

                var eventId = eventData[random2].Id;
                var status = BookingStatus.Pending;

                var processedAt = random1>55 ? createdAt.AddMinutes(f.Random.Int(1, 5)) : (DateTime?)default;
                if (processedAt.HasValue)
                {
                    status = random2 > 100 ? BookingStatus.Confirmed : BookingStatus.Rejected;
                }
                return new Booking(Guid.NewGuid(), status, eventId, createdAt, processedAt);
            });

            _bookings = test_data.UseSeed(8675309).Generate(50).OrderByDescending(u => u.ProcessedAt).ToList();

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
                return _bookings.FirstOrDefault(a => a.Id == id);
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
    }
}
