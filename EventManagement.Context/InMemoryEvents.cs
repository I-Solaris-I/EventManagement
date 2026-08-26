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
    public class InMemoryEvents : IRepository<Event>
    {
        private readonly Lock _lock;

        private List<Event> _events;
        public InMemoryEvents()
        {
            _lock = new();
            _events = new();
            Randomizer.Seed = new Random(8675309);
            var test_data = new Faker<Event>().CustomInstantiator((f) =>
            {
                var startAt = f.Date.Between(DateTime.Now.AddDays(-365), DateTime.Now.AddDays(365));
                var endAt = startAt.AddHours(f.Random.Int(1, 24));
                return new Event(Guid.NewGuid(), f.Lorem.Sentence(), startAt, endAt, f.Random.Number(0, 100) > 30 ? f.Lorem.Paragraph() : null);
                });
                
            _events = test_data.UseSeed(8675309).Generate(100).OrderBy(u => u.StartAt).ToList();

        }
        public void Create(Event data)
        {
            using (_lock.EnterScope())
            {
                _events.Add(new Event(data.Title, data.StartAt, data.EndAt, data.Description));
            }

        }
        public void Update(Event data)
        {
            using (_lock.EnterScope())
            {
                var eventItem = _events.FirstOrDefault(e => e.Id == data.Id);
                if (eventItem != null)
                {
                    eventItem.UpdateEvent(data.Title, data.StartAt, data.EndAt, data.Description);

                }
            }
        }

        public bool IsExist(Guid id)
        {
            using (_lock.EnterScope())
            {
                return _events.Any(a => a.Id == id);
            }
        }

        public IEnumerable<Event> GetAll()
        {
            using (_lock.EnterScope())
            {
                return _events;
            }
        }

        public Event? GetById(Guid id)
        {
            using (_lock.EnterScope())
            {
                return _events.FirstOrDefault(a => a.Id == id);
            }

        }

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
