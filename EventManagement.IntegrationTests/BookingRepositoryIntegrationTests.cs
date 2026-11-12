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
    public class BookingRepositoryIntegrationTests
    {
        DatabaseFixture _fixture;
        public BookingRepositoryIntegrationTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        private CancellationToken ct => TestContext.Current.CancellationToken;
        private IAsyncBookingRepository Sut(AppDbContext context)
        {
            return new BookingRepository(context);
        }
        private async Task SeedBookingAsync(int count)
        {
            await using var ctx = await _fixture.CreateContextAsync();
            var id = Guid.NewGuid();
            ctx.Add(NewTestEvent(id, availableSeats: count, totalSeats: count));
            ctx.AddRange(Enumerable.Range(1, count).Select(i => new Booking(id)));
            await ctx.SaveChangesAsync(ct);
        }
        private static Event NewTestEvent(Guid? id = null, string? title = null, DateTime? start = null, DateTime? end = null, int availableSeats = 5, int totalSeats = 10, string? description = null)
        {
            var now = DateTime.UtcNow;
            return new Event(
                id ?? Guid.NewGuid(),
                title ?? "Новое тестовое мероприятие",
                start ?? now.AddMinutes(10),
                end ?? now.AddMinutes(20),
                totalSeats,
                availableSeats,
                description);
        }




        [Fact]
        public async Task Add_BookingWithDuplicateId_ThrowsDbUpdateException()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();

            await using var context = await _fixture.CreateContextAsync();
            var id = Guid.NewGuid();
            context.Add(NewTestEvent(id));
            context.Add(new Booking(id, BookingStatus.Pending, id, DateTime.UtcNow));
            await context.SaveChangesAsync(ct);

            //Act & Assert
            await using var actContex = await _fixture.CreateContextAsync();
            Sut(actContex).Add(new Booking(id, BookingStatus.Pending, id, DateTime.UtcNow));

            await Assert.ThrowsAsync<DbUpdateException>(() => actContex.SaveChangesAsync(ct));

        }
        [Fact]
        public async Task Add_BookingWhenEventNotExist_ThrowsDbUpdateException()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();

            var id = Guid.NewGuid();

            //Act & Assert
            await using var actContex = await _fixture.CreateContextAsync();
            Sut(actContex).Add(new Booking(id, BookingStatus.Pending, id, DateTime.UtcNow));

            await Assert.ThrowsAsync<DbUpdateException>(() => actContex.SaveChangesAsync(ct));

        }
        [Fact]
        public async Task Add_TwoBookingWithDifferentIds_PersistsBoth()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();

            await using var context = await _fixture.CreateContextAsync();
            var id = Guid.NewGuid();
            context.Add(NewTestEvent(id));
            context.Add(new Booking(id));
            await context.SaveChangesAsync(ct);

            //Act
            await using var actContex = await _fixture.CreateContextAsync();
            Sut(actContex).Add(new Booking(id));
            await actContex.SaveChangesAsync(ct);



            //Assert
            await using var verifyContext = await _fixture.CreateContextAsync();
            var bookings = await verifyContext.Bookings.ToListAsync(ct);
            Assert.Equal(2, bookings.Count);
        }



        [Fact]
        public async Task GetByIdAsync_WhenBookingExists_ReturnsBooking()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();
            var id = Guid.NewGuid();
            await using var context = await _fixture.CreateContextAsync();
            context.Add(NewTestEvent(id));
            var booking = new Booking(id);
            context.Add(booking);
            await context.SaveChangesAsync(ct);

            //Act
            await using var actContext = await _fixture.CreateContextAsync();
            var result = await Sut(actContext).GetByIdAsync(booking.Id, ct);


            //Assert
            Assert.NotNull(result);
            Assert.Equal(booking.Id, result.Id);

        }

        [Fact]
        public async Task Remove_WhenBookingExists_ShouldRemoveAndReturnNull()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();
            var id = Guid.NewGuid();
            await using var context = await _fixture.CreateContextAsync();
            context.Add(NewTestEvent(id));
            var booking = new Booking(id);
            context.Add(booking);
            await context.SaveChangesAsync(ct);

            //Act
            await using var actContext = await _fixture.CreateContextAsync();
            Sut(actContext).Remove(booking.Id);
            await actContext.SaveChangesAsync(ct);

            //Assert
            await using var verifyContext = await _fixture.CreateContextAsync();
            var evt = await verifyContext.Events.FindAsync([booking.Id], ct);
            Assert.Null(evt);
        }

        [Fact]
        public async Task GetByIdAsync_WhenBookingNotExists_ShouldReturnNull()
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



        [Fact]
        public async Task GetPendingAsync_WhenHavePending_ReturnOnlyPending()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            var now = DateTime.UtcNow;

            await using var context = await _fixture.CreateContextAsync();

            var id = Guid.NewGuid();
            context.Add(NewTestEvent(id));
            context.AddRange([new Booking(id), new Booking(id), new Booking(id), new Booking(Guid.NewGuid(), BookingStatus.Confirmed, id, DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow)]);
            await context.SaveChangesAsync(ct);



            // Act
            await using var actContext = await _fixture.CreateContextAsync();
            var result = await Sut(actContext).GetPendingAsync(ct);

            // Assert
            Assert.Equal(3, result.Count());
        }


        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(6)]
        public async Task GetAllAsync_ReturnsExpectedCount(int count)
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            await SeedBookingAsync(count);


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
            context.Add(NewTestEvent(id));
            var booking = new Booking(id);
            context.Add(booking);
            await context.SaveChangesAsync(ct);
            // Act
            await using var actContext = await _fixture.CreateContextAsync();
            var result = await Sut(actContext).IsExistAsync(booking.Id, ct);

            // Assert
            Assert.True(result);
        }



        [Fact]
        public async Task IsExistAsync_WhenBookingNotExists_ReturnFalse()
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
