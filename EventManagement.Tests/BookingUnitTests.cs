using EventManagement.Models;

namespace EventManagement.Tests
{
    /// <summary>
    /// Тесты сущности бронирования
    /// </summary>
    public class BookingUnitTests
    {
        [Fact]
        public void BookingConstructor_EventId_ShouldReturnNewBookingInPending()
        {
            //Arrange
            var eventId = Guid.NewGuid();
            //Act
            var evt = new Booking(eventId);
            //Assert
            Assert.NotNull(evt);
            Assert.Equal(BookingStatus.Pending, evt.Status);
        }

        [Fact]
        public void BookingConstructor_EventId_ShouldNotHaveProcessedAt()
        {
            //Arrange
            var eventId = Guid.NewGuid();
            //Act
            var evt = new Booking(eventId);
            //Assert
            Assert.NotNull(evt);
            Assert.Null(evt.ProcessedAt);
        }

        [Fact]
        public void Confirm_PendingBooking_ShouldHaveProcessedAt()
        {
            //Arrange
            var eventId = Guid.NewGuid();
            var evt = new Booking(eventId);
            //Act
            evt.Confirm();
            //Assert
            Assert.NotNull(evt.ProcessedAt);
        }
        [Fact]
        public void Confirm_PendingBooking_ShoulBeConfirmed()
        {
            //Arrange
            var eventId = Guid.NewGuid();
            var evt = new Booking(eventId);
            //Act
            evt.Confirm();
            //Assert
            Assert.Equal(BookingStatus.Confirmed, evt.Status);
        }
        [Fact]
        public void Reject_PendingBooking_ShouldHaveProcessedAt()
        {
            //Arrange
            var eventId = Guid.NewGuid();
            var evt = new Booking(eventId);
            //Act
            evt.Reject();
            //Assert
            Assert.NotNull(evt.ProcessedAt);
        }
        [Fact]
        public void Reject_PendingBooking_ShouldBeRejected()
        {
            //Arrange
            var eventId = Guid.NewGuid();
            var evt = new Booking(eventId);
            //Act
            evt.Reject();
            //Assert
            Assert.Equal(BookingStatus.Rejected, evt.Status);
        }
        [Fact]
        public void BookingFullContructor_ProcessedAtAndPending_ShouldThrowArgumentException()
        {
            //Arrange
            var bookingId= Guid.NewGuid();
            var utcNow= DateTime.UtcNow;
            var eventId = Guid.NewGuid();
            var createdAt = utcNow.AddMinutes(-1);
            var processedAt = utcNow;
            var status = BookingStatus.Pending;
            //Act 
            var ex = Assert.Throws<ArgumentException>(() => new Booking(bookingId, status, eventId, createdAt, processedAt));
            //Assert
            Assert.Equal("status", ex.ParamName);
        }

        [Fact]
        public void BookingFullContructor_ProcessedAtLessThanCreatedAt_ShouldThrowArgumentException()
        {
            //Arrange
            var bookingId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var eventId = Guid.NewGuid();
            var createdAt = utcNow;
            var processedAt = utcNow.AddMinutes(-1);
            var status = BookingStatus.Confirmed;
            //Act  
            var ex = Assert.Throws<ArgumentException>(() => new Booking(bookingId, status, eventId, createdAt, processedAt));
            //Assert
            Assert.Equal("processedAt", ex.ParamName);
        }

        [Fact]
        public void BookingFullContructor_AllField_ShouldNotHaveEmptyOrNull()
        {
            //Arrange
            var bookingId = Guid.NewGuid();
            var utcNow = DateTime.UtcNow;
            var eventId = Guid.NewGuid();
            var createdAt = utcNow;
            var processedAt = utcNow.AddMinutes(1);
            var status = BookingStatus.Confirmed;
            //Act  
            var booking= new Booking(bookingId, status, eventId, createdAt, processedAt);
            //Assert
            Assert.NotEqual(Guid.Empty,booking.EventId);
            Assert.NotEqual(Guid.Empty, booking.Id);
            Assert.NotEqual(DateTime.MinValue, booking.CreatedAt);
            Assert.Equal(status,booking.Status);
            Assert.NotNull(booking.ProcessedAt);

        }
    }
}
