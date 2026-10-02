using EventManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDTO> CreateBookingAsync(Guid eventId, CancellationToken token = default);
        Task<BookingDTO> GetBookingByIdAsync(Guid bookingId, CancellationToken token = default);
    }
}
