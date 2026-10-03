using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="EventId"></param>
    /// <param name="Status"></param>
    /// <param name="CreatedAt"></param>
    /// <param name="ProcessedAt"></param>
    public record BookingDTO(Guid Id, Guid EventId, BookingStatus Status, DateTime CreatedAt, DateTime? ProcessedAt)
    {

        /// <summary>
        /// Получение модели представления из доменной модели
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static BookingDTO GetModel(Booking e)
        {
            return new BookingDTO(e.Id, e.EventId, e.Status, e.CreatedAt, e.ProcessedAt);
        }

    }
}
