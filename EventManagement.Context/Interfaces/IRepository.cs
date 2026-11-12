using EventManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Context.Interfaces
{
    public interface IAsyncRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        void Add(T data);
        void Remove(Guid id);
        Task<bool> IsExistAsync(Guid id, CancellationToken ct = default);
    }
    public interface IAsyncBookingRepository : IAsyncRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetPendingAsync(CancellationToken ct = default);
    }


    public interface IAsyncEventRepository : IAsyncRepository<Event>
    {
        Task<PaginatedResultDto<Event>> GetAllPaginatedAsync(EventFilterDTO filterData, CancellationToken ct = default);
    }
}
