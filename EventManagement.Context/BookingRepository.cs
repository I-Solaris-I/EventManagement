using EventManagement.Context.DataAccess;
using EventManagement.Context.Interfaces;
using EventManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Context
{
    public class BookingRepository : IAsyncBookingRepository
    {
        private readonly AppDbContext _context;
        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Bookings.ToListAsync(ct);
        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Bookings.FindAsync([id], ct);
        }

        public async Task<IEnumerable<Booking>> GetPendingAsync(CancellationToken ct = default)
        {
            return await _context.Bookings.Where(a => a.Status == BookingStatus.Pending).ToListAsync(ct);
        }

        public async Task<bool> IsExistAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Bookings.AnyAsync(a => a.Id == id);
        }
        public void Add(Booking data)
        {
            _context.Bookings.Add(data);
        }
        public void Remove(Guid id)
        {
            var evt = _context.Bookings.Find([id]);
            if (evt != null)
            {
                _context.Remove(evt);
            }
        }
    }
}
