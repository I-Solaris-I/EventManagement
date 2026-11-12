using EventManagement.Context;
using EventManagement.Context.DataAccess;
using EventManagement.Context.Interfaces;
using EventManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;

namespace EventManagement.IntegrationTests
{
    [Collection("Database")]
    public class EventPersistenceIntegrationTests
    {
        DatabaseFixture _fixture;
        public EventPersistenceIntegrationTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }
        private CancellationToken ct => TestContext.Current.CancellationToken;



        [Fact]
        public async Task UpdateEvent_WhenEventExists_UpdatedEvent()
        {
            //Arrange
            await _fixture.ResetDatabaseAsync();
            var id = Guid.NewGuid();
            await using var context = await _fixture.CreateContextAsync();
            context.Add(new Event(id, "Новое событие", DateTime.UtcNow.AddMinutes(2), DateTime.UtcNow.AddMinutes(5), 10, 5, null));
            await context.SaveChangesAsync(ct);

            //Act
            await using var actContext = await _fixture.CreateContextAsync();
            var evt = await actContext.FindAsync<Event>([id], ct);
            evt!.UpdateEvent("Обновлённое название", evt.StartAt, evt.EndAt, evt.Description);
            await actContext.SaveChangesAsync(ct);

            //Assert
            await using var verifyContext = await _fixture.CreateContextAsync();
            var updated = await verifyContext.Events.FirstAsync(b => b.Id == id, ct);
            Assert.Equal("Обновлённое название", updated.Title);

        }

        [Fact]
        public async Task GetWithInclude_EventWithBookings_ReturnsExpectedBookingsCount()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            var id = Guid.NewGuid();
            await using var context = await _fixture.CreateContextAsync();
            context.Add(new Event(id, "Новое событие", DateTime.UtcNow.AddMinutes(2), DateTime.UtcNow.AddMinutes(5), 10, 5, null));
            context.Add(new Booking(id));
            context.Add(new Booking(id));

            await context.SaveChangesAsync(ct);


            // Act
            await using var actCtx = await _fixture.CreateContextAsync();
            var result = await actCtx.Events.Include(a => a.Bookings).FirstAsync(ct);

            // Assert
            Assert.NotNull(result.Bookings);
            Assert.Equal(2, result.Bookings.Count);

        }
        [Fact]
        public async Task Remove_EventWithBooking_ShouldDeleteBookingsCascade()
        {
            // Arrange
            await _fixture.ResetDatabaseAsync();
            var id = Guid.NewGuid();
            await using var context = await _fixture.CreateContextAsync();
            context.Add(new Event(id, "Новое событие", DateTime.UtcNow.AddMinutes(2), DateTime.UtcNow.AddMinutes(5), 10, 5, null));
            context.Add(new Booking(id));
            context.Add(new Booking(id));

            await context.SaveChangesAsync(ct);


            // Act
            await using var actCtx = await _fixture.CreateContextAsync();
            actCtx.RemoveRange(actCtx.Events);
            await actCtx.SaveChangesAsync(ct);

            // Assert
            await using var verifyCtx = await _fixture.CreateContextAsync();
            var verifyData = await verifyCtx.Bookings.ToListAsync(ct);
            Assert.Empty(verifyData);

        }
    }
}
