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
    /// <summary>
    /// Тесты сервиса управления мероприятиями
    /// </summary>
    public class EventServiceUnitTests
    {
        private CancellationToken ct => TestContext.Current.CancellationToken;
        private readonly Mock<IAsyncEventRepository> _repositoryMock;
        private readonly Mock<ILogger<EventService>> _loggerMock;
        private readonly Mock<IValidator<CreateEventDTO>> _validatorCEMock;
        private readonly Mock<IValidator<UpdateEventDTO>> _validatorUEMock;

        private readonly Mock<IValidator<EventFilterDTO>> _validatorEFMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        private readonly List<Event> _allEvents =
        [
            new Event(
                Guid.Parse("26c5f628-ba61-4281-b106-d473da7c7114"),
                "Illum eum beatae et.",
                DateTime.Parse("2025-09-19T01:14:01.1052829+03:00"),
                DateTime.Parse("2025-09-19T16:14:01.1052829+03:00"),
                10,
                10,
            "Saepe alias rerum repellendus exercitationem ipsam suscipit"
            ),
            new Event(
                Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0"),
                "Aut vel aut repellat aut pariatur exercitationem neque qui in.",
                DateTime.Parse("2025-09-20T10:32:36.6775856+03:00"),
                DateTime.Parse("2025-09-20T22:32:36.6775856+03:00"),
                10,
                10,
                null
            ),
            new Event(
                Guid.Parse("f9f43d97-e81b-4bdf-951f-4e0fb1daee31"),
                "Illum et iste mollitia.",
                DateTime.Parse("2025-09-22T10:48:15.0296976+03:00"),
                DateTime.Parse("2025-09-22T11:48:15.0296976+03:00"),
                10,
                10,
                "Voluptatum dolorem non voluptatem odio maiores minus. Amet nisi est recusandae veniam quasi maxime. Natus alias pariatur eos magni. Hic quo ipsa suscipit voluptas magni nobis et rerum. Et autem quo quaerat eius. Tenetur voluptatem culpa provident."
            ),
            new Event(
                Guid.Parse("d355aeaf-3687-410e-8ea7-6f6993bf12ef"),
                "Inventore numquam quaerat iusto quos quis doloremque consequatur.",
                DateTime.Parse("2025-09-29T09:55:28.956633+03:00"),
                DateTime.Parse("2025-09-30T09:55:28.956633+03:00"),
                10,
                10,
                null),
            new Event(
                Guid.Parse("416466f1-60c1-4897-b414-dd476652ed08"),
                "Error expedita id fugiat ipsam quod et corporis omnis.",
                DateTime.Parse("2025-12-20T03:49:02.0584699+03:00"),
                DateTime.Parse("2025-12-20T16:49:02.0584699+03:00"),
                10,
                10,
                null),
            new Event(
                Guid.Parse("73e4bdc1-d9e0-4cce-8a8d-650ceaf0ad72"),
                "Numquam suscipit labore aliquid magni non ratione consequatur aut maiores.",
                DateTime.Parse("2025-12-23T19:34:22.6389131+03:00"),
                DateTime.Parse("2025-12-24T16:34:22.6389131+03:00"),
                10,
                10,
                null),
            new Event(
                Guid.Parse("ac11f9f0-258e-4a64-998a-2bd847712bf0"),
                "Tempore soluta suscipit porro.",
                DateTime.Parse("2025-12-31T05:42:56.9818145+03:00"),
                DateTime.Parse("2025-12-31T16:42:56.9818145+03:00"),
                10,
                10,
                "Alias veniam delectus quis sed dolores. Inventore et et eos qui qui ratione cumque pariatur. Tenetur enim totam quam voluptas soluta. Laudantium natus aspernatur. Dolorem vero aut non unde consequatur ut."
            ),
            new Event(
                Guid.Parse("4a105c2d-8af3-4b04-ac2b-88353340ec3a"),
                "At omnis assumenda distinctio voluptas nihil.",
                DateTime.Parse("2026-01-09T16:51:33.2872767+03:00"),
                DateTime.Parse("2026-01-10T15:51:33.2872767+03:00"),
                10,
                10,
                "Quos tenetur exercitationem nihil reiciendis. Est quos aperiam repellat. Ratione sunt dolorem ipsa pariatur ut facere. Ut sapiente quo voluptates sit tenetur. Nulla ex et fuga qui. Quia ducimus quidem est nisi quam sapiente."
            ),
            new Event(
                Guid.Parse("3824b4fb-20c2-4732-b035-f84cc656e044"),
                "Minus sunt sint",
                DateTime.Parse("2026-01-24T13:06:00.5533601+03:00"),
                DateTime.Parse("2026-01-24T22:06:00.5533601+03:00"),
                10,
                10,
                null
            ),
            new Event(
                Guid.Parse("9e3cc89a-c672-4da7-a2fa-bb25919a2b7f"),
                "Minus sunt sint1",
                DateTime.Parse("2026-01-29T07:24:31.3202369+03:00"),
                DateTime.Parse("2026-01-29T14:24:31.3202369+03:00"),
                10,
                10,
                null
            ),
            new Event(
                Guid.Parse("8092b0dd-27e0-426e-99d1-e1bf6ee75dcf"),
                "Minus sunt sint2",
                DateTime.Parse("2026-02-06T15:54:08.4935775+03:00"),
                DateTime.Parse("2026-02-06T22:54:08.4935775+03:00"),
                10,
                10,
                "Eius illum modi aut expedita id accusamus iure. Eum et corrupti ut corrupti inventore. Quae officia iste mollitia fuga libero. Delectus mollitia sed."
            ),
            new Event(
                Guid.Parse("aef4d001-0c87-4d2f-ba03-6345443ee0b1"),
                "Minus sunt sint3",
                DateTime.Parse("2026-02-08T10:56:57.5048028+03:00"),
                DateTime.Parse("2026-02-08T14:56:57.5048028+03:00"),
                10,
                10,
                "Quas fugiat minima quia dolor sed molestiae earum eaque natus. Et id dignissimos nihil ipsa. Explicabo sit reiciendis omnis qui ipsum. Odio porro ratione. Magni dolore non itaque repudiandae quia voluptatem nulla atque. In aspernatur itaque quis omnis suscipit similique iste laudantium cumque."
            ),
            new Event(
                Guid.Parse("b667bcb1-6fcf-4bb6-b86e-f16097e19cd5"),
                "Accusantium sunt cum laudantium quod consequatur laudantium possimus asperiores ea.",
                DateTime.Parse("2026-02-08T19:14:38.4989543+03:00"),
                DateTime.Parse("2026-02-09T08:14:38.4989543+03:00"),
                10,
                10,
                "Aliquam qui consectetur necessitatibus et qui sunt. Repellat architecto quia. Accusamus consectetur et totam ratione."
            ),
            new Event(
                Guid.Parse("a34fd67d-e192-4a35-9aa1-a752f83fd010"),
                "Nisi voluptatum harum quibusdam dolorem.",
                DateTime.Parse("2026-02-10T07:38:43.1405658+03:00"),
                DateTime.Parse("2026-02-10T19:38:43.1405658+03:00"),
                10,
                10,
                "Est assumenda quia autem ipsa quam hic tenetur. Praesentium eos tempore dolorem. Labore cumque autem accusantium necessitatibus libero occaecati ipsum aliquid. Tenetur rerum voluptatibus nesciunt nemo recusandae placeat voluptatem tempora suscipit. Velit nihil autem voluptatem molestiae deserunt asperiores rerum. Et qui molestias natus labore non nihil et aut."
            )
        ];


        public EventServiceUnitTests()
        {
            _repositoryMock = new Mock<IAsyncEventRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<EventService>>();

            _repositoryMock.Setup(a => a.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_allEvents);
            _repositoryMock.Setup(a => a.IsExistAsync(Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0"), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _repositoryMock.Setup(a => a.GetByIdAsync(Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0"), It.IsAny<CancellationToken>())).ReturnsAsync(_allEvents[1]);

            _validatorCEMock = new Mock<IValidator<CreateEventDTO>>();
            _validatorUEMock = new Mock<IValidator<UpdateEventDTO>>();
            _validatorEFMock = new Mock<IValidator<EventFilterDTO>>();
        }
        private EventService Sut(IAsyncEventRepository? repository = null, ILogger<EventService>? logger = null, IValidator<CreateEventDTO>? validatorCE = null, IValidator<UpdateEventDTO>? validatorUE = null, IValidator<EventFilterDTO>? validatorEF = null, IUnitOfWork? uow = null)
        {
            return new EventService(
                repository ?? _repositoryMock.Object,
                logger ?? _loggerMock.Object,
                validatorCE ?? _validatorCEMock.Object,
                validatorUE ?? _validatorUEMock.Object,
                validatorEF ?? _validatorEFMock.Object,
                uow ?? _unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetAllEvents_ShouldReturnAllExistingEvents()
        {
            //Arrange
            var eventService = Sut();
            //Act
            var result = await eventService.GetAllEventsAsync(ct);

            //Assert
            Assert.NotNull(result);
            _repositoryMock.Verify(a => a.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(_allEvents.Count, result.Count);
        }

    




        [Fact]
        public async Task GetEventById_ExistingGuid_ShouldReturnEventWithSameGuid()
        {
            //Arrange
            var existingGuid = Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0");
            var eventService = Sut();
            //Act
            var result = await eventService.GetEventByIdAsync(existingGuid, ct);
            //Assert
            Assert.NotNull(result);
            Assert.Equal(existingGuid, result.Id);
            _repositoryMock.Verify(a => a.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetEventById_NotExistingGuid_ShouldThrowEventNotFoundedExeption()
        {
            //Arrange
            var notExistingGuid = Guid.Parse("c2cfdfd8-e38d-4e0f-bcf2-bb3206c632c0");
            var eventService = Sut();
            //Act && Assert
            await Assert.ThrowsAsync<EventNotFoundedException>(() => eventService.GetEventByIdAsync(notExistingGuid, ct));
            _repositoryMock.Verify(a => a.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateEvent_CorrectValues_ShouldBecreatedAndReturnNotEmptyGuid()
        {
            //Arrange
            var model = new CreateEventDTO(
                "Veritatis voluptatum rerum.",
                DateTime.UtcNow.AddMinutes(5),
                DateTime.UtcNow.AddMinutes(30),
                10,
                "Ducimus ea minus nam sit"
                );

            _validatorCEMock.Setup(x => x.Validate(model)).Returns(new ValidationResult());

            var eventService = Sut();


            //Act
            var result = await eventService.CreateEventAsync(model, ct);

            //Assert
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(ct), Times.Once);
            _repositoryMock.Verify(
                  x => x.Add(It.Is<Event>(e =>
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
        public async Task UpdateEvent_CorrectValues_ShouldBeUpdated()
        {
            //Arrange
            var existedID = Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0");
            var model = new UpdateEventDTO(
                        "Veritatis voluptsatum rerum.",
                        DateTime.Parse("2025-11-02T05:01:05.8051311+03:00"),
                        DateTime.Parse("2026-11-02T06:01:05.8051311+03:00"),
                        "Ducimus ea minus nam sit"
                        );

            _repositoryMock.Setup(a => a.GetByIdAsync(Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0"), It.IsAny<CancellationToken>())).ReturnsAsync(_allEvents[1]);
            _validatorUEMock.Setup(x => x.Validate(model)).Returns(new ValidationResult());

            var eventService = Sut();

            //Act
            await eventService.UpdateEventAsync(existedID, model, ct);

            //Assert
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(ct), Times.Once);

            Assert.Equal(model.Title, _allEvents[1].Title);
            Assert.Equal(model.StartAt, _allEvents[1].StartAt);
            Assert.Equal(model.EndAt, _allEvents[1].EndAt);
            Assert.Equal(model.Description, _allEvents[1].Description);


        }

        [Fact]
        public async Task UpdateEvent_NotExistedGuid_ShouldThrowEventNotFoundedExeption()
        {
            //Arrange

            var notExistedGuid = Guid.Parse("c8cfafa8-e38d-4e0f-bcf2-bb20e6c632c0");
            var model = new UpdateEventDTO(
                        "Veritatis voluptsatum rerum.",
                        DateTime.Parse("2025-11-02T05:01:05.8051311+03:00"),
                        DateTime.Parse("2026-11-02T06:01:05.8051311+03:00"),
                        "Ducimus ea minus nam sit"
                        );

            _validatorUEMock.Setup(x => x.Validate(It.IsAny<UpdateEventDTO>())).Returns(new ValidationResult());
            var eventService = Sut();


            //Act && Assert
            await Assert.ThrowsAsync<EventNotFoundedException>(() => eventService.UpdateEventAsync(notExistedGuid, model, ct));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(ct), Times.Never);

        }
        [Fact]
        public async Task CreateEvent_ModelIsNull_ShouldThrowArgumentNullException()
        {
            //Arrange
            var model = (CreateEventDTO?)default;
            var eventService = Sut();


            //Act && Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => eventService.CreateEventAsync(model!, ct));
            _repositoryMock.Verify(
        x => x.Add(It.IsAny<Event>()),
        Times.Never);

            _validatorCEMock.Verify(
                x => x.Validate(It.IsAny<CreateEventDTO>()),
                Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(ct), Times.Never);

        }





        [Fact]
        public async Task RemoveEvent_ExistedGuid_MustBeСompleted()
        {
            //Arrange
            var existedGuid = Guid.Parse("c9cfdfd8-e38d-4e0f-bcf2-bb20e6c632c0");
            var eventService = Sut();

            //Act
            await eventService.RemoveEventAsync(existedGuid, ct);

            //Assert
            _repositoryMock.Verify(a => a.Remove(existedGuid), Times.Once);
        }

        [Fact]
        public async Task RemoveEvent_NotExistedGuid_ShouldThrowEventNotFoundedExeption()
        {
            //Arrange
            var notExistedGuid = Guid.Parse("c9cfdfd8-e38d-4e0f-baf2-bb20e6c632c0");
            var eventService = Sut();

            //Act && Assert
            await Assert.ThrowsAsync<EventNotFoundedException>(() => eventService.RemoveEventAsync(notExistedGuid, ct));
            _repositoryMock.Verify(a => a.Remove(notExistedGuid), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(ct), Times.Never);

        }


        [Fact]
        public async Task CreateEvent_InvalidDate_ShouldThrowValidationExeption()
        {
            //Arrange
            var dto = new CreateEventDTO("Новое мероприятие", DateTime.MinValue, DateTime.UtcNow, 10);
            var eventService = Sut(validatorCE: new CreateEventDTOValidation());

            //Act && Assert
            await Assert.ThrowsAsync<ValidationException>(() => eventService.CreateEventAsync(dto, TestContext.Current.CancellationToken));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(ct), Times.Never);

        }

        [Fact]
        public async Task UpdateEvent_InvalidDate_ShouldThrowValidationExeption()
        {
            //Arrange
            var evt = _allEvents.First();
            var updData = new UpdateEventDTO("Новое название", DateTime.UtcNow, DateTime.MinValue);
            var eventService = Sut(validatorUE: new UpdateEventDTOValidation());

            //Act && Assert
            await Assert.ThrowsAsync<ValidationException>(() => eventService.UpdateEventAsync(evt.Id, updData, ct));
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(ct), Times.Never);

        }
    }
}
