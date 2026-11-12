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
    public class EventRepository : IAsyncEventRepository
    {
        private readonly AppDbContext _context;
        public EventRepository(AppDbContext context)
        {
            _context = context;
        }
        public void Add(Event data)
        {
            _context.Events.Add(data);
        }

        public void Remove(Guid id)
        {
            var evt = _context.Events.Find([id]);
            if (evt != null)
            {
                _context.Remove(evt);
            }
        }

        public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Events.ToListAsync(ct);
        }

        public async Task<PaginatedResultDto<Event>> GetAllPaginatedAsync(EventFilterDTO filterData, CancellationToken ct = default)
        {
            DateTime _from = DateTime.MinValue;
            string _title = string.Empty;
            DateTime _to = DateTime.MinValue;
            bool hasTitleFiltration = false, hasFromFiltration = false, hasToFiltration = false;

            if (!string.IsNullOrEmpty(filterData.Title))
            {
                _title = filterData.Title;
                hasTitleFiltration = true;
            }
            if (filterData.From.HasValue)
            {
                _from = filterData.From!.Value.ToUniversalTime();
                hasFromFiltration = true;
            }
            if (filterData.To.HasValue)
            {
                _to = filterData.To!.Value.ToUniversalTime();
                hasToFiltration = true;
            }

            var query = _context.Events.AsQueryable();

            if (hasFromFiltration)
            {
                query = query.Where(a => a.StartAt >= _from);
            }
            if (hasToFiltration)
            {
                query = query.Where(a => a.EndAt <= _to);
            }
            if (hasTitleFiltration)
            {
                var titleFilter = _title.ToLower();
                
                query = query.Where(a => a.Title.ToLower().Contains(titleFilter));
            }
            var total = await query.CountAsync(ct);
            var items = await query
                .Skip((filterData.Page - 1) * filterData.PageSize)
                .Take(filterData.PageSize)
                .ToListAsync(ct);

            return new PaginatedResultDto<Event>()
            {
                Items = items,
                PageSize = filterData.PageSize,
                Page = filterData.Page,
                Total = total
            };
        }

        public async Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Events.FindAsync([id], ct);
        }

        public async Task<bool> IsExistAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Events.AnyAsync(a => a.Id == id, ct);
        }

    }
}
