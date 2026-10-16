
using EventManagement.Context;
using EventManagement.Context.Interfaces;
using EventManagement.Models;
using EventManagement.Services;
using Microsoft.Extensions.Logging;
using Moq;


namespace EventManagement.Tests
{
    public class BookingServiceConcurrencyUnitTest
    {

        private readonly IRepository<Event> _eventRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly Mock<ILogger<BookingService>> _loggerMock;
        public BookingServiceConcurrencyUnitTest()
        {
            _eventRepository = new InMemoryEvents();
            _bookingRepository = new InMemoryBooking(_eventRepository);
            _loggerMock = new Mock<ILogger<BookingService>>();
        }
        private BookingService Sut(IBookingRepository? bookingRepository = null, IRepository<Event>? eventRepository = null, ILogger<BookingService>? logger = null)
        {
            return new BookingService(
                bookingRepository ?? _bookingRepository,
                eventRepository ?? _eventRepository,
                logger ?? _loggerMock.Object);
        }

        [Fact]
        public async Task CreateBookingAsync_TwentyConcurrentCallsForTenSeats_TenSucceedAndTenFailWithNoSeats()
        {
            var evt = new Event(Guid.NewGuid(), "Новое мероприятие", DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), 10, 10);

            _eventRepository.Create(evt);
            var br = Sut();

            var tasks = Enumerable.Range(1, 20).Select(elem => Task.Run(() => br.CreateBookingAsync(evt.Id, default), default)).ToList();
            Task all = Task.WhenAll(tasks);
            try
            {
                await all;
            }
            catch { }
            var cmlQty = tasks.Count(a => a.IsCompletedSuccessfully);
            var faultedQty = tasks.Count(a => a.IsFaulted);
            var exseptions = tasks.Where(a => a.IsFaulted).Select(a => a.Exception!.InnerException).ToList();
            Assert.Equal(10, cmlQty);
            Assert.Equal(tasks.Count, cmlQty + faultedQty);
            Assert.All(exseptions, ex => Assert.IsType<EventNoAvailableSeatsException>(ex));
        }

        [Fact]
        public async Task CreateBookingAsync_WhenTenConcurrentRequests_CreatesTenUniqueBookings()
        {

            var evt = new Event(Guid.NewGuid(), "Новое мероприятие", DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), 10, 10);

            _eventRepository.Create(evt);
            var br = Sut();

            var tasks = Enumerable.Range(1, 10).Select(elem => Task.Run(() => br.CreateBookingAsync(evt.Id, default), default)).ToList();
            Task all = Task.WhenAll(tasks);
            try
            {
                await all;
            }
            catch{ }
            var cmlQty = tasks.Count(a => a.IsCompletedSuccessfully);
            var ids = tasks.Select(a => a.Result.Id).Distinct().ToHashSet();
            Assert.Equal(10, cmlQty);
            Assert.Equal(10, ids.Count);
        }


    }
}
