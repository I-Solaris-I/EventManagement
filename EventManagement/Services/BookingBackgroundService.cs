using EventManagement.Context.Interfaces;
using EventManagement.Models;
using EventManagement.Services.Interfaces;
using Microsoft.OpenApi;

namespace EventManagement.Services
{
    /// <summary>
    /// Сервис управления статусом бронирований мероприятий
    /// </summary>
    public class BookingBackgroundService : BackgroundService
    {
        private const int _delayApiSec = 2;
        private const int _delaySecInWhile = 1;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingBackgroundService> _logger;
        /// <summary>
        /// Конструктор <see cref="BookingBackgroundService"/>
        /// </summary>
        /// <param name="scopeFactory"></param>
        /// <param name="logger"></param>
        public BookingBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BookingBackgroundService> logger)
        {
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
                    await Task.Delay(TimeSpan.FromSeconds(_delaySecInWhile), stoppingToken);
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
        /// <param name="stoppingToken">Токен отмены</param>
        /// <returns></returns>
        private async Task ProcessBookingsAsync(CancellationToken stoppingToken)
        {

            await using var scope = _scopeFactory.CreateAsyncScope();

            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

            foreach (var bId in bookingRepository.GetAll().Where(a => a.Status == BookingStatus.Pending).Select(a => a.Id).ToArray())
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_delayApiSec));
                    var updBooking = await bookingService.ProcessPendingBookingAsync(bId, stoppingToken);

                    _logger.LogInformation("Cтатус брони {BookingId} изменён на {Status}", bId, updBooking.Status);

                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Исключение при обработке брони {BookingId}", bId);
                }
            }
        }
    }
}