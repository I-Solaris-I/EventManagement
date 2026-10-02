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

    }
}
