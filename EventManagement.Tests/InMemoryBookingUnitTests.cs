using EventManagement.Context;
using EventManagement.Context.Interfaces;
using EventManagement.Models;
using Moq;


namespace EventManagement.Tests
{

    public class InMemoryBookingFixture
    {
        private readonly Mock<IRepository<Event>> _eventReposiroryMock;
        public IReadOnlyCollection<Event> AllEvents { get; private set; } =
        [
            new Event(
                Guid.Parse("26c5f628-ba61-4281-b106-d473da7c7114"),
                "Illum eum beatae et.",
                DateTime.Parse("2025-09-19T01:14:01.1052829+03:00"),
                DateTime.Parse("2025-09-19T16:14:01.1052829+03:00"),
            "Saepe alias rerum repellendus exercitationem ipsam suscipit"
            ),
            new Event(
                Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0"),
                "Aut vel aut repellat aut pariatur exercitationem neque qui in.",
                DateTime.Parse("2025-09-20T10:32:36.6775856+03:00"),
                DateTime.Parse("2025-09-20T22:32:36.6775856+03:00"),
                null
            ),
            new Event(
                Guid.Parse("f9f43d97-e81b-4bdf-951f-4e0fb1daee31"),
                "Illum et iste mollitia.",
                DateTime.Parse("2025-09-22T10:48:15.0296976+03:00"),
                DateTime.Parse("2025-09-22T11:48:15.0296976+03:00"),
                "Voluptatum dolorem non voluptatem odio maiores minus. Amet nisi est recusandae veniam quasi maxime. Natus alias pariatur eos magni. Hic quo ipsa suscipit voluptas magni nobis et rerum. Et autem quo quaerat eius. Tenetur voluptatem culpa provident."
            ),
            new Event(
                Guid.Parse("d355aeaf-3687-410e-8ea7-6f6993bf12ef"),
                "Inventore numquam quaerat iusto quos quis doloremque consequatur.",
                DateTime.Parse("2025-09-29T09:55:28.956633+03:00"),
                DateTime.Parse("2025-09-30T09:55:28.956633+03:00"),
                null),
            new Event(
                Guid.Parse("416466f1-60c1-4897-b414-dd476652ed08"),
                "Error expedita id fugiat ipsam quod et corporis omnis.",
                DateTime.Parse("2025-12-20T03:49:02.0584699+03:00"),
                DateTime.Parse("2025-12-20T16:49:02.0584699+03:00"),
                null),
            new Event(
                Guid.Parse("73e4bdc1-d9e0-4cce-8a8d-650ceaf0ad72"),
                "Numquam suscipit labore aliquid magni non ratione consequatur aut maiores.",
                DateTime.Parse("2025-12-23T19:34:22.6389131+03:00"),
                DateTime.Parse("2025-12-24T16:34:22.6389131+03:00"),
                null)];
        public InMemoryBooking BookingRepository { get; set; }

        public InMemoryBookingFixture()
        {
            _eventReposiroryMock = new Mock<IRepository<Event>>();
            _eventReposiroryMock.Setup(a => a.GetAll()).Returns(AllEvents);

            BookingRepository = new InMemoryBooking(_eventReposiroryMock.Object);
        }
    }
    /// <summary>
    /// Тесты репозитория бронирований мероприятий, который хранится в памяти
    /// </summary>
    public class InMemoryBookingUnitTests : IClassFixture<InMemoryBookingFixture>
    {
        private readonly IReadOnlyCollection<Event> _allEvents;
        private readonly IRepository<Booking> _repository;

        private Booking CreateTestBooking()
        {
            return new Booking(_allEvents.First().Id);
        }

        public InMemoryBookingUnitTests(InMemoryBookingFixture fixture)
        {
            _repository = fixture.BookingRepository;
            _allEvents = fixture.AllEvents;
        }
        [Fact]
        public void GetAll_NotEmpty()
        {
            //Arrange

            //Act
            var res = _repository.GetAll();
            Assert.IsAssignableFrom<IEnumerable<Booking>>(res);
            Assert.NotEmpty(res);
        }

        [Fact]
        public void GetById_ExistingId_ShouldRetunBookingWithExistingId()
        {
            //Arrange
            var booking = CreateTestBooking();
            //Act
            _repository.Create(booking);
            var res = _repository.GetById(booking.Id);
            // Assert
            Assert.NotNull(res);
            Assert.Equal(booking.Id, res.Id);
        }
        [Fact]
        public void GetById_NotExistingId_ShouldReturnNull()
        {
            // Arrange
            var testGuid = Guid.Empty;
            // Act
            var result = _repository.GetById(testGuid);
            // Assert
            Assert.Null(result);
        }


        [Fact]
        public void IsExist_NotExistingId_ShouldReturnFalse()
        {            
            // Arrange
            var guid = Guid.Empty;
            // Act
            var result = _repository.IsExist(guid);
            // Assert
            Assert.False(result);
        }
        [Fact]
        public void IsExist_ExistingId_ShouldReturnTrue()
        {
            // Arrange
            var booking = CreateTestBooking();
            // Act
            _repository.Create(booking);
            var result = _repository.IsExist(booking.Id);
            // Assert
            Assert.True(result);
        }


        [Fact]
        public void Delete_ExistingId_ShouldRemoveBooking()
        {
            // Arrange
            var booking = CreateTestBooking();            
            // Act && Assert
            _repository.Create(booking);
            Assert.True(_repository.IsExist(booking.Id));
            _repository.Delete(booking.Id);
            Assert.False(_repository.IsExist(booking.Id));
            Assert.Null(_repository.GetById(booking.Id));
        }
        [Fact]
        public void Delete_NotExistingId_ShouldNotThrow()
        {
            // Arrange
            var guid = Guid.Empty;
            // Act
                var exception = Record.Exception(() =>
                   _repository.Delete(Guid.NewGuid()));
            //Assert
            Assert.Null(exception);
        }
    }
}
