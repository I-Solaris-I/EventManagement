
using EventManagement.Context;
using EventManagement.Context.DataAccess;
using EventManagement.Context.Interfaces;
using EventManagement.Models;
using EventManagement.Services;
using EventManagement.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;


namespace EventManagement.Tests
{
    public class BookingServiceConcurrencyUnitTest : IDisposable
    {
        private readonly IAsyncEventRepository _eventRepository;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;

        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;

        public BookingServiceConcurrencyUnitTest()
        {

            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IAsyncBookingRepository, BookingRepository>();
            services.AddScoped<IAsyncEventRepository, EventRepository>();
            services.AddScoped<IUnitOfWork>(services => services.GetRequiredService<AppDbContext>());
            services.AddScoped<ILogger<BookingService>>(_ => new Mock<ILogger<BookingService>>().Object);
            services.AddScoped<ILogger<EventService>>(_ => new Mock<ILogger<EventService>>().Object);
            services.AddScoped<IValidator<CreateEventDTO>, CreateEventDTOValidation>();
            services.AddScoped<IValidator<UpdateEventDTO>, UpdateEventDTOValidation>();
            services.AddScoped<IValidator<EventFilterDTO>, EventFilterDTOValidation>();

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
            _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();

            _eventRepository = _scope.ServiceProvider.GetRequiredService<IAsyncEventRepository>();
        }

        CancellationToken ct => TestContext.Current.CancellationToken;
        [Fact]
        public async Task CreateBookingAsync_TwentyConcurrentCallsForTenSeats_TenSucceedAndTenFailWithNoSeats()
        {
            var guid = await _eventService.CreateEventAsync(new CreateEventDTO("Новое мероприятие", DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), 10), TestContext.Current.CancellationToken);



            var tasks = Enumerable.Range(1, 20).Select(elem => Task.Run(() => _bookingService.CreateBookingAsync(guid, default), default)).ToList();
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
            Assert.Equal(0, (await _eventRepository.GetByIdAsync(guid, ct))!.AvailableSeats);
            Assert.Equal(tasks.Count, cmlQty + faultedQty);
            Assert.All(exseptions, ex => Assert.IsType<EventNoAvailableSeatsException>(ex));
        }

        [Fact]
        public async Task CreateBookingAsync_WhenTenConcurrentRequests_CreatesTenUniqueBookings()
        {

            var guid = await _eventService.CreateEventAsync(new CreateEventDTO("Новое мероприятие", DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(6), 10), TestContext.Current.CancellationToken);


            var tasks = Enumerable.Range(1, 10).Select(elem => Task.Run(() => _bookingService.CreateBookingAsync(guid, ct), ct)).ToList();
            Task all = Task.WhenAll(tasks);
            try
            {
                await all;
            }
            catch { }
            var cmlQty = tasks.Count(a => a.IsCompletedSuccessfully);
            var ids = tasks.Select(a => a.Result.Id).Distinct().ToHashSet();
            Assert.Equal(10, cmlQty);
            Assert.Equal(10, ids.Count);
        }

        public void Dispose()
        {
            _scope.Dispose();
        }
    }
}
