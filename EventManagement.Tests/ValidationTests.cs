using EventManagement.Models;
using FluentValidation.TestHelper;
namespace EventManagement.Tests
{
    public class ValidatorTests
    {
        EventFilterDTOValidation validatorEF;

        CreateUpdateEventDTOValidation validatorCUE;

        public ValidatorTests()
        {
            validatorEF = new();
            validatorCUE = new();

        }

        [Fact]
        [Trait("Category", nameof(CreateUpdateEventDTOValidation))]

        public void CreateUpdateEventDTO_Title_EmtyTitle_ShouldHaveValidationError()
        {
            //Arrange
            var title = string.Empty;
            var startAt = DateTime.Parse("2026-09-09");
            var endAt = DateTime.Parse("2025-09-10");
            var model = new CreateUpdateEventDTO(title, startAt, endAt);
            //Act
            var result = validatorCUE.TestValidate(model);
            //Assert
            result.ShouldHaveValidationErrorFor(a => a.Title);
        }

        [Fact]
        [Trait("Category", nameof(CreateUpdateEventDTOValidation))]
        public void CreateUpdateEventDTO_EndAt_EarlierThanStartAt_ShouldHaveValidationError()
        {

            //Arrange
            var title = "Новое мероприятие";
            var startAt = DateTime.Parse("2026-09-09");
            var endAt = DateTime.Parse("2025-03-05");
            var model = new CreateUpdateEventDTO(title, startAt, endAt);
            //Act
            var result = validatorCUE.TestValidate(model);
            //Assert
            result.ShouldHaveValidationErrorFor(a => a.EndAt);
        }

        [Fact]
        [Trait("Category", nameof(CreateUpdateEventDTOValidation))]

        public void CreateUpdateEventDTO_StartAt_MinValue_ShouldHaveValidationError()
        {

            //Arrange
            var title = "Новое мероприятие";
            var startAt = DateTime.MinValue;
            var endAt = DateTime.Parse("2026-03-05");
            var model = new CreateUpdateEventDTO(title, startAt, endAt);
            //Act
            var result = validatorCUE.TestValidate(model);
            //Assert
            result.ShouldHaveValidationErrorFor(a => a.StartAt).WithErrorMessage("Не указана дата начала мероприятия");
        }
        [Fact]
        [Trait("Category", nameof(CreateUpdateEventDTOValidation))]

        public void CreateUpdateEventDTO_CorrectValues_ShouldNotHaveAnyValidationError()
        {
            //Arrange
            var model = new CreateUpdateEventDTO("Новое мероприятие", DateTime.Parse("2025-03-04"), DateTime.Parse("2025-03-05"));
            //Act
            var result = validatorCUE.TestValidate(model);
            //Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
        [Theory]
        [InlineData(-100)]
        [InlineData(-50)]
        [InlineData(0)]
        [Trait("Category", nameof(CreateUpdateEventDTOValidation))]
        public void EventFilterDTO_Page_InvalidValues_ShouldHaveValidationError(int page)
        {
            //Arrange
            var model = new EventFilterDTO(Page: page);
            //Act
            var result = validatorEF.TestValidate(model);
            //Assert
            result.ShouldHaveValidationErrorFor(a => a.Page).WithErrorMessage("Номер страницы должен быть неотрицательным");
        }

        [Theory]
        [InlineData(-100)]
        [InlineData(-20)]
        [InlineData(1001)]
        [Trait("Category", nameof(EventFilterDTOValidation))]
        public void EventFilterDTO_PageSize_InvalidValues_ShouldHaveValidationError(int pageSize)
        {
            //Arrange
            var model = new EventFilterDTO(PageSize: pageSize);
            //Act
            var result = validatorEF.TestValidate(model);
            //Assert
            result.ShouldHaveValidationErrorFor(a => a.PageSize);

        }
        [Fact]
        [Trait("Category", nameof(EventFilterDTOValidation))]
        public void EventFilterDTO_Title_EmtyTitle_ShouldHaveValidationError()
        {
            //Arrange
            var title = "";
            var model = new EventFilterDTO(Title: title);
            //Act
            var result = validatorEF.TestValidate(model);
            //Assert
            result.ShouldHaveValidationErrorFor(a => a.Title);

        }
        [Fact]
        [Trait("Category", nameof(EventFilterDTOValidation))]
        public void EventFilterDTO_To_EarlierThanFrom_ShouldHaveValidationError()
        {
            //Arrange
            var from = DateTime.Parse("2024-01-01");
            var to = DateTime.Parse("2023-02-02");
            var model = new EventFilterDTO(From: from, To: to);
            //Act
            var result = validatorEF.TestValidate(model);
            //Assert
            result.ShouldHaveValidationErrorFor(a => a.To);

        }
        [Fact]
        [Trait("Category", nameof(EventFilterDTOValidation))]
        public void EventFilterDTO_CorrectValues_ShouldNotHaveAnyValidationError()
        {
            //Arrange
            var title = "Заголовок фильтрации";
            var from = DateTime.Parse("2026-09-01");
            var to = DateTime.Parse("2026-09-02");
            var page = 1;
            var pageSize = 50;
            var model = new EventFilterDTO(title, from, to, page, pageSize);
            //Act
            var result = validatorEF.TestValidate(model);
            //Assert
            result.ShouldNotHaveAnyValidationErrors();

        }
    }
}
