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
    /// Репозиторий мероприятий
    /// </summary>
    public class InMemoryEvents : IRepository<Event>
    {
        private const int fakerSeed = 8675309;
        private const int generateCount = 100;
        private const int randomSetDescriprion = 30;
        private readonly Lock _lock;

        private List<Event> _events;
        /// <summary>
        /// Конструктор
        /// </summary>
        public InMemoryEvents()
        {
            _lock = new();
            var test_data = new Faker<Event>().CustomInstantiator((f) =>
              {
                  var startAt = f.Date.Between(DateTime.UtcNow.AddDays(-365), DateTime.UtcNow.AddDays(365));
                  var endAt = startAt.AddHours(f.Random.Int(1, 24));
                  return new Event(f.Random.Guid(), f.Lorem.Sentence(), startAt, endAt, f.Random.Number(0, 100) > randomSetDescriprion ? f.Lorem.Paragraph() : null);
              });
            _events = test_data.UseSeed(fakerSeed).Generate(generateCount).OrderByDescending(u => u.StartAt).ToList();

        }
        /// <summary>
        /// Создание мероприятия
        /// </summary>
        /// <param name="data">Данные для создания</param>
        public void Create(Event data)
        {
            using (_lock.EnterScope())
            {
                _events.Add(data);
            }

        }
        /// <summary>
        /// Обновление мероприятия
        /// </summary>
        /// <param name="data">Данные для обновления</param>
        public void Update(Event data)
        {
            using (_lock.EnterScope())
            {
                var eventItem = _events.FirstOrDefault(e => e.Id == data.Id);
                if (eventItem != null)
                {
                    _events.Remove(eventItem);
                    _events.Add(data);
                }
            }
        }
        /// <summary>
        /// Проверить, существует ли мероприятие без его возврата
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <returns></returns>
        public bool IsExist(Guid id)
        {
            using (_lock.EnterScope())
            {
                return _events.Any(a => a.Id == id);
            }
        }
        /// <summary>
        /// Получить все мероприятия
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Event> GetAll()
        {
            using (_lock.EnterScope())
            {
                return _events.ToList();
            }
        }
        /// <summary>
        /// Получение мероприятия по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        /// <returns></returns>
        public Event? GetById(Guid id)
        {
            using (_lock.EnterScope())
            {
                var evt= _events.FirstOrDefault(a => a.Id == id);
                if (evt != null)
                {
                    return new Event(evt.Id, evt.Title, evt.StartAt, evt.EndAt, evt.Description);
                }
                else return null;
            }

        }
        /// <summary>
        /// Удаление мероприятия
        /// </summary>
        /// <param name="id">Идентификатор мероприятия</param>
        public void Delete(Guid id)
        {
            using (_lock.EnterScope())
            {
                var eventItem = _events.FirstOrDefault(e => e.Id == id);
                if (eventItem != null)
                {
                    _events.Remove(eventItem);
                }
            }
        }
    }
}
