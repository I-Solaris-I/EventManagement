using EventManagement.Context.Interfaces;
using EventManagement.Models;
using EventManagement.Services;
using EventManagement.Services.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventManagement.Tests
{
    public class EventServiceUnitTests
    {
        private readonly Mock<IRepository<Event>> _repositoryMock;
        private readonly Mock<ILogger<EventService>> _loggerMock;
        private readonly Mock<IValidator<CreateUpdateEventDTO>> _validatorCUEMock;
        private readonly Mock<IValidator<EventFilterDTO>> _validatorEFMock;
        private readonly List<Event> _allEvents =
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
                null),
            new Event(
                Guid.Parse("ac11f9f0-258e-4a64-998a-2bd847712bf0"),
                "Tempore soluta suscipit porro.",
                DateTime.Parse("2025-12-31T05:42:56.9818145+03:00"),
                DateTime.Parse("2025-12-31T16:42:56.9818145+03:00"),
                "Alias veniam delectus quis sed dolores. Inventore et et eos qui qui ratione cumque pariatur. Tenetur enim totam quam voluptas soluta. Laudantium natus aspernatur. Dolorem vero aut non unde consequatur ut."
            ),
            new Event(
                Guid.Parse("4a105c2d-8af3-4b04-ac2b-88353340ec3a"),
                "At omnis assumenda distinctio voluptas nihil.",
                DateTime.Parse("2026-01-09T16:51:33.2872767+03:00"),
                DateTime.Parse("2026-01-10T15:51:33.2872767+03:00"),
                "Quos tenetur exercitationem nihil reiciendis. Est quos aperiam repellat. Ratione sunt dolorem ipsa pariatur ut facere. Ut sapiente quo voluptates sit tenetur. Nulla ex et fuga qui. Quia ducimus quidem est nisi quam sapiente."
            ),
            new Event(
                Guid.Parse("3824b4fb-20c2-4732-b035-f84cc656e044"),
                "Minus sunt sint",
                DateTime.Parse("2026-01-24T13:06:00.5533601+03:00"),
                DateTime.Parse("2026-01-24T22:06:00.5533601+03:00"),
                null
            ),
            new Event(
                Guid.Parse("9e3cc89a-c672-4da7-a2fa-bb25919a2b7f"),
                "Minus sunt sint1",
                DateTime.Parse("2026-01-29T07:24:31.3202369+03:00"),
                DateTime.Parse("2026-01-29T14:24:31.3202369+03:00"),
                null
            ),
            new Event(
                Guid.Parse("8092b0dd-27e0-426e-99d1-e1bf6ee75dcf"),
                "Minus sunt sint2",
                DateTime.Parse("2026-02-06T15:54:08.4935775+03:00"),
                DateTime.Parse("2026-02-06T22:54:08.4935775+03:00"),
                "Eius illum modi aut expedita id accusamus iure. Eum et corrupti ut corrupti inventore. Quae officia iste mollitia fuga libero. Delectus mollitia sed."
            ),
            new Event(
                Guid.Parse("aef4d001-0c87-4d2f-ba03-6345443ee0b1"),
                "Minus sunt sint3",
                DateTime.Parse("2026-02-08T10:56:57.5048028+03:00"),
                DateTime.Parse("2026-02-08T14:56:57.5048028+03:00"),
                "Quas fugiat minima quia dolor sed molestiae earum eaque natus. Et id dignissimos nihil ipsa. Explicabo sit reiciendis omnis qui ipsum. Odio porro ratione. Magni dolore non itaque repudiandae quia voluptatem nulla atque. In aspernatur itaque quis omnis suscipit similique iste laudantium cumque."
            ),
            new Event(
                Guid.Parse("b667bcb1-6fcf-4bb6-b86e-f16097e19cd5"),
                "Accusantium sunt cum laudantium quod consequatur laudantium possimus asperiores ea.",
                DateTime.Parse("2026-02-08T19:14:38.4989543+03:00"),
                DateTime.Parse("2026-02-09T08:14:38.4989543+03:00"),
                "Aliquam qui consectetur necessitatibus et qui sunt. Repellat architecto quia. Accusamus consectetur et totam ratione."
            ),
            new Event(
                Guid.Parse("a34fd67d-e192-4a35-9aa1-a752f83fd010"),
                "Nisi voluptatum harum quibusdam dolorem.",
                DateTime.Parse("2026-02-10T07:38:43.1405658+03:00"),
                DateTime.Parse("2026-02-10T19:38:43.1405658+03:00"),
                "Est assumenda quia autem ipsa quam hic tenetur. Praesentium eos tempore dolorem. Labore cumque autem accusantium necessitatibus libero occaecati ipsum aliquid. Tenetur rerum voluptatibus nesciunt nemo recusandae placeat voluptatem tempora suscipit. Velit nihil autem voluptatem molestiae deserunt asperiores rerum. Et qui molestias natus labore non nihil et aut."
            )
        ];


        public EventServiceUnitTests()
        {
            _repositoryMock = new Mock<IRepository<Event>>();

            _loggerMock = new Mock<ILogger<EventService>>();

            _repositoryMock.Setup(a => a.GetAll()).Returns(_allEvents);
            _repositoryMock.Setup(a => a.IsExist(Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0"))).Returns(true);
            _repositoryMock.Setup(a => a.GetById(Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0"))).Returns(_allEvents[1]);

            _validatorCUEMock = new Mock<IValidator<CreateUpdateEventDTO>>();
            _validatorEFMock = new Mock<IValidator<EventFilterDTO>>();

        }

        [Fact]
        public void GetAllEvents_ShouldReturnAllExistingEvents()
        {
            //Arrange
            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);
            //Act
            var result = eventService.GetAllEvents();

            //Assert
            Assert.NotNull(result);
            _repositoryMock.Verify(a => a.GetAll(), Times.Once);
            Assert.Equal(_allEvents.Count, result.Count);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(2, 20)]
        [InlineData(1, 1000)]
        [InlineData(3, 5000)]
        public void GetFilteredEvents_ValidPageAndPageSize_ReturnsCorrectNumberOfItemsAndPage(int page, int pageSize)
        {
            //Arrange
            _validatorEFMock.Setup(a => a.Validate(It.IsAny<EventFilterDTO>())).Returns(new ValidationResult { });
            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);
            //Act
            var result = eventService.GetFilteredEvents(new EventFilterDTO { Page = page, PageSize = pageSize });
            //Assert
            Assert.NotNull(result);
            Assert.True(result.Items.Count() <= result.PageSize);
            Assert.Equal(result.Count, result.Items.Count());
            Assert.Equal(page, result.Page);

            Assert.Equal(pageSize, result.PageSize);

        }



        [Fact]
        public void GetEventById_ExistingGuid_ShouldReturnEventWithSameGuid()
        {
            //Arrange
            var existingGuid = Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0");
            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);
            //Act
            var result = eventService.GetEventById(existingGuid);
            //Assert
            Assert.NotNull(result);
            Assert.Equal(existingGuid, result.Id);
            _repositoryMock.Verify(a => a.GetById(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void GetEventById_NotExistingGuid_ShouldThrowEventNotFoundedExeption()
        {
            //Arrange
            var notExistingGuid = Guid.Parse("c2cfdfd8-e38d-4e0f-bcf2-bb3206c632c0");
            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);
            //Act && Assert
            Assert.Throws<EventNotFoundedExeption>(() => eventService.GetEventById(notExistingGuid));
            _repositoryMock.Verify(a => a.IsExist(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public void CreateEvent_CorrectValues_ShouldBecreatedAndReturnNotEmptyGuid()
        {
            //Arrange
            var model = new CreateUpdateEventDTO(
                "Veritatis voluptatum rerum.",
                DateTime.Parse("2025-11-02T05:01:05.8051311+03:00"),
                DateTime.Parse("2026-11-02T06:01:05.8051311+03:00"),
                "Ducimus ea minus nam sit"
                );

            _validatorCUEMock.Setup(x => x.Validate(model)).Returns(new ValidationResult());

            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);


            //Act
            var result = eventService.CreateEvent(model);

            //Assert
            _repositoryMock.Verify(
                  x => x.Create(It.Is<Event>(e =>
                      e.Title == model.Title &&
                      e.StartAt == model.StartAt &&
                      e.EndAt == model.EndAt &&
                      e.Description == model.Description
                  )),
                  Times.Once);
            Assert.IsAssignableFrom<Guid>(result);
            Assert.NotEqual(Guid.Empty, result);
        }

        [Fact]
        public void UpdateEvent_CorrectValues_ShouldBeUpdated()
        {
            //Arrange
            var existedID = Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0");
            var model = new CreateUpdateEventDTO(
                        "Veritatis voluptsatum rerum.",
                        DateTime.Parse("2025-11-02T05:01:05.8051311+03:00"),
                        DateTime.Parse("2026-11-02T06:01:05.8051311+03:00"),
                        "Ducimus ea minus nam sit"
                        );

            _validatorCUEMock.Setup(x => x.Validate(model)).Returns(new ValidationResult());

            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);

            //Act
            eventService.UpdateEvent(existedID, model);

            //Assert
            _repositoryMock.Verify(
                  x => x.Update(It.Is<Event>(e =>
                      e.Title == model.Title &&
                      e.StartAt == model.StartAt &&
                      e.EndAt == model.EndAt &&
                      e.Description == model.Description
                  )),
                  Times.Once);
        }

        [Fact]
        public void UpdateEvent_NotExistedGuid_ShouldThrowEventNotFoundedExeption()
        {
            //Arrange

            var notExistedGuid = Guid.Parse("c8cfafa8-e38d-4e0f-bcf2-bb20e6c632c0");

            var model = new CreateUpdateEventDTO(
                        "Veritatis voluptsatum rerum.",
                        DateTime.Parse("2025-11-02T05:01:05.8051311+03:00"),
                        DateTime.Parse("2026-11-02T06:01:05.8051311+03:00"),
                        "Ducimus ea minus nam sit"
                        );

            _validatorCUEMock.Setup(x => x.Validate(It.IsAny<CreateUpdateEventDTO>())).Returns(new ValidationResult());

            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);


            //Act && Assert
            Assert.Throws<EventNotFoundedExeption>(() => eventService.UpdateEvent(notExistedGuid, model));

            _repositoryMock.Verify(
                  x => x.Update(It.IsAny<Event>()),
                  Times.Never);
        }
        [Fact]
        public void CreateEvent_ModelIsNull_ShouldThrowArgumentNullException()
        {
            //Arrange
            var model = (CreateUpdateEventDTO?)default;

            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);


            //Act && Assert
            Assert.Throws<ArgumentNullException>(() => eventService.CreateEvent(model!));
            _repositoryMock.Verify(
        x => x.Create(It.IsAny<Event>()),
        Times.Never);

            _validatorCUEMock.Verify(
                x => x.Validate(It.IsAny<CreateUpdateEventDTO>()),
                Times.Never);
        }

        [Fact]
        public void GetFilteredEvents_filterText_ShouldHaveOnlyFilteredItems()
        {
            //Arrange
            var filterText = "illum e";

            _validatorEFMock.Setup(x => x.Validate(It.IsAny<EventFilterDTO>())).Returns(new ValidationResult());

            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);

            //Act
            var result = eventService.GetFilteredEvents(new EventFilterDTO(filterText));

            //Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count());
            Assert.All(result.Items, x => Assert.True(x.Title.Contains(filterText, StringComparison.InvariantCultureIgnoreCase)));
        }

        public static IEnumerable<object[]> FilterCases =>
        [
                     ["Minus sunt sint", new DateTime(2026, 1, 3), new DateTime(2026, 1, 31)],
                     [ "Illum e", new DateTime(2025, 9, 1), new DateTime(2025, 9, 30) ],

        ];

        [Theory]
        [MemberData(nameof(FilterCases))]
        public void GetFilteredEvents_AllFilters_ShouldHaveOnlyFilteredItems(string filterText, DateTime from, DateTime to)
        {
            //Arrange

            _validatorEFMock.Setup(x => x.Validate(It.IsAny<EventFilterDTO>())).Returns(new ValidationResult());

            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);

            //Act
            var result = eventService.GetFilteredEvents(new EventFilterDTO(filterText, from, to));

            //Assert
            Assert.NotNull(result);
            Assert.All(result.Items, x => Assert.True(x.Title.Contains(filterText, StringComparison.InvariantCultureIgnoreCase) && x.StartAt >= from && x.EndAt <= to));

        }
        [Fact]
        public void RemoveEvent_ExistedGuid_MustBeСompleted()
        {
            //Arrange
            var existedGuid = Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0");


            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);

            //Act
            eventService.RemoveEvent(existedGuid);

            //Assert
            _repositoryMock.Verify(a => a.Delete(existedGuid), Times.Once);
        }

        [Fact]
        public void RemoveEvent_NotExistedGuid_ShouldThrowEventNotFoundedExeption()
        {
            //Arrange
            var notExistedGuid = Guid.Parse("c9cfdfd8-e38d-4e0f-baf2-bb20e6c632c0");


            var eventService = new EventService(_repositoryMock.Object, _loggerMock.Object, _validatorCUEMock.Object, _validatorEFMock.Object);

            //Act && Assert
            Assert.Throws<EventNotFoundedExeption>(() => eventService.RemoveEvent(notExistedGuid));
            _repositoryMock.Verify(a => a.Delete(notExistedGuid), Times.Never);
        }
    }
}
