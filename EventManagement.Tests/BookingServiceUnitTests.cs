using EventManagement.Context.Interfaces;
using EventManagement.Models;
using EventManagement.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventManagement.Tests
{
    /// <summary>
    /// Тесты сервиса бронирования мероприятий
    /// </summary>
    public class BookingServiceUnitTests
    {
        private readonly Mock<IBookingRepository> _repositoryBookingMock;
        private readonly Mock<IRepository<Event>> _repositoryEventMock;
        private readonly Mock<ILogger<BookingService>> _loggerMock;
        private readonly List<Booking> _allBookings =
        [
            new Booking(
                Guid.Parse("26c5f628-4444-2222-b106-337aaa7c7114"),
                BookingStatus.Pending,
                Guid.Parse("26c5f628-ba61-4281-b106-d473da7c7114"),
                DateTime.Parse("2025-09-20T10:32:36.6775856+03:00"),
                default
            ),
             new Booking(
                Guid.Parse("d355aeaf-bc20-4281-b106-d473da7c7114"),
                BookingStatus.Pending,
                Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0"),
                DateTime.Parse("2025-09-20T10:32:36.6775856+03:00"),
                default
            ),
            new Booking(
                Guid.Parse("4e0fb1da-ba61-951f-b106-e81bda7c7114"),
                BookingStatus.Pending,
                Guid.Parse("f9f43d97-e81b-4bdf-951f-4e0fb1daee31"),
                DateTime.Parse("2025-09-20T10:32:36.6775856+03:00"),
                default
            ),
        ];
        private readonly List<Event> _allEvents =
        [
        new Event(
               Guid.Parse("26c5f628-ba61-4281-b106-d473da7c7114"),
                "Illum eum beatae et.",
                DateTime.Parse("2035-09-30T01:14:01.1052829+03:00"),
                DateTime.Parse("2035-09-30T16:14:01.1052829+03:00"),
            "Saepe alias rerum repellendus exercitationem ipsam suscipit"
            ),
            new Event(
                Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0"),
                "Aut vel aut repellat aut pariatur exercitationem neque qui in.",
                DateTime.Parse("2025-03-20T10:32:36.6775856+03:00"),
                DateTime.Parse("2025-03-20T22:32:36.6775856+03:00"),
                null
            ),
            new Event(
                Guid.Parse("f9f43d97-e81b-4bdf-951f-4e0fb1daee31"),
                "Illum et iste mollitia.",
                DateTime.Parse("2025-11-22T10:48:15.0296976+03:00"),
                DateTime.Parse("2025-11-22T11:48:15.0296976+03:00"),
                "Voluptatum dolorem non voluptatem odio maiores minus. Amet nisi est recusandae veniam quasi maxime. Natus alias pariatur eos magni. Hic quo ipsa suscipit voluptas magni nobis et rerum. Et autem quo quaerat eius. Tenetur voluptatem culpa provident."
            ),
            new Event(
                Guid.Parse("d355aeaf-3687-410e-8ea7-6f6993bf12ef"),
                "Inventore numquam quaerat iusto quos quis doloremque consequatur.",
                DateTime.Parse("2025-12-29T09:55:28.956633+03:00"),
                DateTime.Parse("2025-12-30T09:55:28.956633+03:00"),
                null)];

        public BookingServiceUnitTests()
        {
            _repositoryBookingMock = new Mock<IBookingRepository>();
            _repositoryEventMock = new Mock<IRepository<Event>>();
            _loggerMock = new Mock<ILogger<BookingService>>();
            _repositoryBookingMock.Setup(a => a.GetAll()).Returns(_allBookings);
            _repositoryEventMock.Setup(a => a.GetAll()).Returns(_allEvents);
        }

        [Fact]
        public async Task CreateBookingAsync_ExistEventId_ShouldReturnBookingInPending()
        {
            //Arrange
            var testGuid = _allEvents[0].Id;
            _repositoryEventMock.Setup(a => a.GetById(testGuid)).Returns(_allEvents[0]);

            var bookingService = new BookingService(_repositoryBookingMock.Object, _repositoryEventMock.Object, _loggerMock.Object);
            //Act
            var result = await bookingService.CreateBookingAsync(testGuid, TestContext.Current.CancellationToken);

            //Assert
            Assert.NotNull(result);
            Assert.NotEqual(BookingStatus.Rejected, result.Status);
            Assert.NotEqual(BookingStatus.Confirmed, result.Status);

            Assert.Equal(BookingStatus.Pending, result.Status);
        }


        [Fact]
        public async Task CreateBookingAsync_SameExisingtEventId_ShouldReturnBookingWithDifferentId()
        {
            //Arrange
            var testGuid = _allEvents[0].Id;
            _repositoryEventMock.Setup(a => a.GetById(testGuid)).Returns(_allEvents[0]);

            var bookingService = new BookingService(_repositoryBookingMock.Object, _repositoryEventMock.Object, _loggerMock.Object);
            //Act
            var result1 = await bookingService.CreateBookingAsync(testGuid, TestContext.Current.CancellationToken);
            var result2 = await bookingService.CreateBookingAsync(testGuid, TestContext.Current.CancellationToken);
            var result3 = await bookingService.CreateBookingAsync(testGuid, TestContext.Current.CancellationToken);

            //Assert
            Assert.NotEqual(result1.Id, result2.Id);
            Assert.NotEqual(result2.Id, result3.Id);
            Assert.NotEqual(result1.Id, result3.Id);

        }
        [Fact]
        public async Task CreateBookingAsync_NotExistingEventId_ShouldReturnEventNotFoundException()
        {
            //Arrange
            var testGuid = Guid.NewGuid();
            _repositoryEventMock.Setup(a => a.GetById(testGuid)).Returns((Event?)null);
            var bookingService = new BookingService(_repositoryBookingMock.Object, _repositoryEventMock.Object, _loggerMock.Object);

            //Act && Assert
            await Assert.ThrowsAsync<EventNotFoundedException>(() => bookingService.CreateBookingAsync(testGuid, TestContext.Current.CancellationToken));

        }

        [Fact]
        public async Task CreateBookingAsync_StartEventDateLessThanUtcNow_ShouldReturnBookingBusinessException()
        {
            //Arrange
            var testGuid = _allEvents[1].Id;
            _repositoryEventMock.Setup(a => a.GetById(testGuid)).Returns(_allEvents[1]);

            var bookingService = new BookingService(_repositoryBookingMock.Object, _repositoryEventMock.Object, _loggerMock.Object);

            //Act && Assert
            await Assert.ThrowsAsync<BookingBusinessException>(() => bookingService.CreateBookingAsync(testGuid, TestContext.Current.CancellationToken));

        }
        [Fact]
        public async Task GetBookingByIdAsync_ExistingId_ShouldReturnBooking()
        {
            //Arrange
            var testGuid = _allBookings[0].Id;
            _repositoryBookingMock.Setup(a => a.GetById(testGuid)).Returns(_allBookings[0]);

            var bookingService = new BookingService(_repositoryBookingMock.Object, _repositoryEventMock.Object, _loggerMock.Object);

            //Act 
            var result = await bookingService.GetBookingByIdAsync(testGuid, TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(result);
            _repositoryBookingMock.Verify(a => a.GetById(testGuid), Times.Once());
        }




        [Fact]
        public async Task ProcessPendingBookingAsync_WhenBookingNotFound_ShouldThrow()
        {
            // Arrange
            var bookingId = Guid.NewGuid();

            _repositoryBookingMock
                .Setup(x => x.GetById(bookingId))
                .Returns((Booking?)null);
            var bookingService = new BookingService(_repositoryBookingMock.Object, _repositoryEventMock.Object, _loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<BookingNotFoundedException>(
                () => bookingService.ProcessPendingBookingAsync(bookingId, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ProcessPendingBookingAsync_WhenBookingIsNotPending_ShouldThrowBookingBusinessException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();

            var booking = new Booking(
                bookingId,
                BookingStatus.Confirmed,
                Guid.NewGuid(),
                DateTime.UtcNow,
                null);

            _repositoryBookingMock
                .Setup(x => x.GetById(bookingId))
                .Returns(booking);
            var bookingService = new BookingService(_repositoryBookingMock.Object, _repositoryEventMock.Object, _loggerMock.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BookingBusinessException>(
                () => bookingService.ProcessPendingBookingAsync(bookingId, TestContext.Current.CancellationToken));

            Assert.Contains(
                BookingStatus.Pending.ToString(),
                exception.Message);
        }

        [Fact]
        public async Task ProcessPendingBookingAsync_WhenEventNotFound_ShouldRejectBooking()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var booking = new Booking(
                bookingId,
                BookingStatus.Pending,
                eventId,
                DateTime.UtcNow,
                null);

            var rejectedBooking = new Booking(
                bookingId,
                BookingStatus.Rejected,
                eventId,
                booking.CreatedAt,
                DateTime.UtcNow);

            _repositoryBookingMock
                .Setup(x => x.GetById(bookingId))
                .Returns(booking);

            _repositoryEventMock
                .Setup(x => x.GetById(eventId))
                .Returns((Event?)null);

            _repositoryBookingMock
                .Setup(x => x.Reject(bookingId))
                .Returns(rejectedBooking);
            var bookingService = new BookingService(_repositoryBookingMock.Object, _repositoryEventMock.Object, _loggerMock.Object);

            // Act
            var result = await bookingService.ProcessPendingBookingAsync(bookingId, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(BookingStatus.Rejected, result.Status);

            _repositoryBookingMock.Verify(
                x => x.Reject(bookingId),
                Times.Once);

            _repositoryBookingMock.Verify(
                x => x.Confirm(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessPendingBookingAsync_WhenEventAlreadyStarted_ShouldRejectBooking()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var booking = new Booking(
                bookingId,
                BookingStatus.Pending,
                eventId,
                DateTime.UtcNow,
                null);

            var evt = new Event(eventId, "Название", DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(-5));


            var rejectedBooking = new Booking(
                bookingId,
                BookingStatus.Rejected,
                eventId,
                booking.CreatedAt,
                DateTime.UtcNow);

            _repositoryBookingMock
                .Setup(x => x.GetById(bookingId))
                .Returns(booking);

            _repositoryEventMock
                .Setup(x => x.GetById(eventId))
                .Returns(evt);

            _repositoryBookingMock
                .Setup(x => x.Reject(bookingId))
                .Returns(rejectedBooking);

            var bookingService = new BookingService(_repositoryBookingMock.Object, _repositoryEventMock.Object, _loggerMock.Object);

            // Act
            var result = await bookingService.ProcessPendingBookingAsync(bookingId, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(BookingStatus.Rejected, result.Status);

            _repositoryBookingMock.Verify(
                x => x.Reject(bookingId),
                Times.Once);

            _repositoryBookingMock.Verify(
                x => x.Confirm(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessPendingBookingAsync_WhenEventHasNotStarted_ShouldConfirmBooking()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var booking = new Booking(
                bookingId,
                BookingStatus.Pending,
                eventId,
                DateTime.UtcNow,
                null);

            var evt = new Event(eventId, "Название", DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));


            var confirmedBooking = new Booking(
                bookingId,
                BookingStatus.Confirmed,
                eventId,
                booking.CreatedAt,
                DateTime.UtcNow);

            _repositoryBookingMock
                .Setup(x => x.GetById(bookingId))
                .Returns(booking);

            _repositoryEventMock
                .Setup(x => x.GetById(eventId))
                .Returns(evt);

            _repositoryBookingMock
                .Setup(x => x.Confirm(bookingId))
                .Returns(confirmedBooking);
            
            var bookingService = new BookingService(_repositoryBookingMock.Object, _repositoryEventMock.Object, _loggerMock.Object);

            // Act
            var result = await bookingService.ProcessPendingBookingAsync(bookingId,TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(BookingStatus.Confirmed, result.Status);

            _repositoryBookingMock.Verify(
                x => x.Confirm(bookingId),
                Times.Once);

            _repositoryBookingMock.Verify(
                x => x.Reject(It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessPendingBookingAsync_WhenConfirmReturnsNull_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            var booking = new Booking(
                bookingId,
                BookingStatus.Pending,
                eventId,
                DateTime.UtcNow,
                null);

            var evt = new Event(eventId, "Название", DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));


            _repositoryBookingMock
                .Setup(x => x.GetById(bookingId))
                .Returns(booking);

            _repositoryEventMock
                .Setup(x => x.GetById(eventId))
                .Returns(evt);

            _repositoryBookingMock
                .Setup(x => x.Confirm(bookingId))
                .Returns((Booking?)null);
            var bookingService = new BookingService(_repositoryBookingMock.Object, _repositoryEventMock.Object, _loggerMock.Object);


            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => bookingService.ProcessPendingBookingAsync(bookingId, TestContext.Current.CancellationToken));
        }

    }
}
