using EventManagement.Context.DataAccess;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;
using EventManagement.Context.Interfaces;
using EventManagement.Context;
using EventManagement.Models;

namespace EventManagement.IntegrationTests
{
    [Collection("Database")]
    public class EventRepositoryIntegrationTests
    {
        private readonly DatabaseFixture _fixture;

        public EventRepositoryIntegrationTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }


        private CancellationToken ct => TestContext.Current.CancellationToken;
        private IAsyncEventRepository Sut(AppDbContext context)
        {
            return new EventRepository(context);
        }
        private async Task SeedEventsAsync(int count)
        {
            await using var ctx = await _fixture.CreateContextAsync();
            ctx.AddRange(Enumerable.Range(1, count).Select(i => NewTestEvent($"Событие {i}")));
            await ctx.SaveChangesAsync(ct);
        }
        private static Event NewTestEvent(string? title = null, DateTime? start = null, DateTime? end = null, int availableSeats = 5, int totalSeats = 10, string? description = null, Guid? Id = null)
        {
            var now = DateTime.UtcNow;
            return new Event(
                Id ?? Guid.NewGuid(),
                title ?? "Новое тестовое мероприятие",
                start ?? now.AddMinutes(10),
                end ?? now.AddMinutes(20),
                totalSeats,
                availableSeats,
                description);
        }




        [Fact]
        public async Task Add_EventWithDuplicateId_ThrowsDbUpdateException()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();

            await using var context = await _fixture.CreateContextAsync();
            var id = Guid.NewGuid();
            context.Add(new Event(id, "Новое событие", DateTime.UtcNow.AddMinutes(2), DateTime.UtcNow.AddMinutes(5), 10, 5, null));
            await context.SaveChangesAsync(ct);

            //Act & Assert
            await using var actContex = await _fixture.CreateContextAsync();
            Sut(actContex).Add(new Event(id, "Новое событие", DateTime.UtcNow.AddMinutes(2), DateTime.UtcNow.AddMinutes(5), 10, 5, null));

            await Assert.ThrowsAsync<DbUpdateException>(() => actContex.SaveChangesAsync(ct));

        }
        [Fact]
        public async Task Add_TwoEventsWithDifferentIds_PersistsBoth()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();

            await using var context = await _fixture.CreateContextAsync();
            context.Add(new Event(Guid.NewGuid(), "Новое событие", DateTime.UtcNow.AddMinutes(2), DateTime.UtcNow.AddMinutes(5), 10, 5, null));
            await context.SaveChangesAsync(ct);

            //Act
            await using var actContex = await _fixture.CreateContextAsync();
            Sut(actContex).Add(new Event(Guid.NewGuid(), "Новое событие", DateTime.UtcNow.AddMinutes(2), DateTime.UtcNow.AddMinutes(5), 10, 5, null));
            await actContex.SaveChangesAsync(ct);



            //Assert
            await using var verifyContext = await _fixture.CreateContextAsync();
            var events = await verifyContext.Events.ToListAsync(ct);
            Assert.Equal(2, events.Count);
        }

        [Fact]
        public async Task Add_WhenTitleExceedsMaxLength_ThrowsDbUpdateException()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();

            //Act & Assert
            await using var actContex = await _fixture.CreateContextAsync();
            Sut(actContex).Add(new Event(Guid.NewGuid(), new string('s', 51), DateTime.UtcNow.AddMinutes(2), DateTime.UtcNow.AddMinutes(5), 10, 5, null));

            await Assert.ThrowsAsync<DbUpdateException>(() => actContex.SaveChangesAsync(ct));

        }

        [Fact]
        public async Task GetByIdAsync_WhenEventExists_ReturnsEvent()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();
            var id = Guid.NewGuid();
            await using var context = await _fixture.CreateContextAsync();
            context.Add(new Event(id, "Новое событие", DateTime.UtcNow.AddMinutes(2), DateTime.UtcNow.AddMinutes(5), 10, 5, null));
            await context.SaveChangesAsync(ct);

            //Act
            await using var actContext = await _fixture.CreateContextAsync();
            var result = await Sut(actContext).GetByIdAsync(id, ct);


            //Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);

        }

        [Fact]
        public async Task Remove_WhenEventExists_ShouldRemoveAndReturnNull()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();
            var id = Guid.NewGuid();
            await using var context = await _fixture.CreateContextAsync();
            context.Add(new Event(id, "Новое событие", DateTime.UtcNow.AddMinutes(2), DateTime.UtcNow.AddMinutes(5), 10, 5, null));
            await context.SaveChangesAsync(ct);

            //Act
            await using var actContext = await _fixture.CreateContextAsync();
            Sut(actContext).Remove(id);
            await actContext.SaveChangesAsync(ct);

            //Assert
            await using var verifyContext = await _fixture.CreateContextAsync();
            var evt = await verifyContext.Events.FindAsync([id], ct);
            Assert.Null(evt);
        }

        [Fact]
        public async Task GetByIdAsync_WhenEventNotExists_ShouldReturnNull()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();
            var id = Guid.NewGuid();

            //Act
            await using var actContext = await _fixture.CreateContextAsync();
            var result = await Sut(actContext).GetByIdAsync(id, ct);

            //Assert
            Assert.Null(result);

        }

        [Theory]
        [InlineData(1, 10, 3)] // всё на одной странице
        [InlineData(1, 2, 2)]  // первая страница, 2 из 3
        [InlineData(2, 2, 1)]  // вторая страница, последний элемент
        [InlineData(3, 2, 0)]  // за пределами — пусто
        public async Task GetAllPaginatedAsync_PaginationEdgeCases_ReturnsExpectedCount(int page, int pageSize, int expectedCount)
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await SeedEventsAsync(count: 3);

            var filter = new EventFilterDTO { Page = page, PageSize = pageSize };

            // Act
            await using var actCtx = await _fixture.CreateContextAsync();
            var result = await Sut(actCtx).GetAllPaginatedAsync(filter, ct);

            // Assert
            Assert.Equal(3, result.Total);
            Assert.Equal(expectedCount, result.Items.Count);
            Assert.Equal(page, result.Page);
            Assert.Equal(pageSize, result.PageSize);
        }

        [Fact]
        public async Task GetAllPaginatedAsync_WithFilterAndPagination_TotalReflectsFilteredCount()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await using (var ctx = await _fixture.CreateContextAsync())
            {
                ctx.AddRange(
                    NewTestEvent("Концерт 1"),
                    NewTestEvent("Концерт 2"),
                    NewTestEvent("Концерт 3"),
                    NewTestEvent("Выставка"));
                await ctx.SaveChangesAsync(ct);
            }

            var filter = new EventFilterDTO { Title = "Концерт", Page = 1, PageSize = 2 };

            // Act
            await using var actCtx = await _fixture.CreateContextAsync();
            var result = await Sut(actCtx).GetAllPaginatedAsync(filter, ct);

            // Assert
            Assert.Equal(3, result.Total);
            Assert.Equal(2, result.Items.Count);
        }
        [Fact]
        public async Task GetAllPaginatedAsync_FromToFilter_TotalReflectsFilteredCount()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            var now = DateTime.UtcNow;
            await using (var ctx = await _fixture.CreateContextAsync())
            {
                ctx.AddRange(
                    NewTestEvent("Концерт 1", start: now.AddMinutes(10), end: now.AddMinutes(30)),
                    NewTestEvent("Концерт 2", start: now.AddMinutes(5), end: now.AddMinutes(25)),
                    NewTestEvent("Концерт 3", start: now.AddMinutes(1), end: now.AddMinutes(30)),
                    NewTestEvent("Выставка 1", start: now.AddMinutes(50), end: now.AddMinutes(150)));
                await ctx.SaveChangesAsync(ct);
            }

            var filter = new EventFilterDTO { From = now.AddMinutes(2), To = now.AddMinutes(26) };

            // Act
            await using var actCtx = await _fixture.CreateContextAsync();
            var result = await Sut(actCtx).GetAllPaginatedAsync(filter, ct);

            // Assert
            Assert.Equal(1, result.Total);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(6)]
        public async Task GetAllAsync_ReturnsExpectedCount(int count)
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await SeedEventsAsync(count);


            // Act
            await using var actCtx = await _fixture.CreateContextAsync();
            var result = await Sut(actCtx).GetAllAsync(ct);

            // Assert
            Assert.Equal(count, result.Count());
        }

        [Fact]
        public async Task IsExistAsync_WhenBookingExists_ReturnTrue()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            var id = Guid.NewGuid();
            await using var context = await _fixture.CreateContextAsync();
            context.Add(NewTestEvent(Id: id));
            await context.SaveChangesAsync(ct);
            // Act
            await using var actContext = await _fixture.CreateContextAsync();
            var result = await Sut(actContext).IsExistAsync(id, ct);

            // Assert
            Assert.True(result);
        }



        [Fact]
        public async Task IsExistAsync_WhenEventNotExists_ReturnFalse()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            var id = Guid.NewGuid();

            // Act
            await using var actContext = await _fixture.CreateContextAsync();
            var result = await Sut(actContext).IsExistAsync(id, ct);

            // Assert
            Assert.False(result);
        }

    }
}
