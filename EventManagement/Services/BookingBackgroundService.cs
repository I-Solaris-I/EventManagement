using EventManagement.Context.Interfaces;
using EventManagement.Models;
using Microsoft.OpenApi;

namespace EventManagement.Services
{
    /// <summary>
    /// Сервис управления статусом бронирований мероприятий
    /// </summary>
    public class BookingBackgroundService : BackgroundService
    {
        private readonly Random random;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingBackgroundService> _logger;
        /// <summary>
        /// Конструктор <see cref="BookingBackgroundService"/>
        /// </summary>
        /// <param name="scopeFactory"></param>
        /// <param name="logger"></param>
        public BookingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BookingBackgroundService> logger)
        {
            random = new Random();
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        /// <summary>
        /// Метод запуска
        /// </summary>
        /// <param name="stoppingToken">токен остановки сервиса</param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(BookingBackgroundService)} запущен в {DateTime.UtcNow}");
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessBookingsAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                //Нормальное завершение работы
            }
            finally
            {
                _logger.LogInformation($"{nameof(BookingBackgroundService)} остановлен");

            }
        }

        /// <summary>
        /// Обработка бронирований
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        private async Task ProcessBookingsAsync(CancellationToken stoppingToken)
        {

            await using var scope = _scopeFactory.CreateAsyncScope();

            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            foreach (var booking in bookingRepository.GetAll().Where(a => a.Status == BookingStatus.Pending))
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    if (random.Next(0, 100) >= 10)
                        booking.Confirm();
                    else
                        booking.Reject();

                    bookingRepository.Update(booking);
                    _logger.LogInformation("Cтатус брони {BookingId} изменён на {Status}", booking.Id, booking.Status.ToString());

                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Исключение при обработке брони {BookingId}", booking.Id);
                }
            }
        }
    }
}