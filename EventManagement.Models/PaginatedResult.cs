using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Models
{
    /// <summary>
    /// Результат пагинации
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginatedResultDto<T>
    {
        /// <summary>
        /// Элементы
        /// </summary>
        public List<T> Items { get; set; } = new();
        /// <summary>
        /// Всего элементов
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// Страница
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// Количество элементов на текущей странице (фактически)
        /// </summary>
        public int Count => Items.Count;
        /// <summary>
        /// Всего страниц
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
        /// <summary>
        /// Есть ли следующая
        /// </summary>
        public bool HasNext => Page < TotalPages;
        /// <summary>
        /// Есть ли  предыдущая
        /// </summary>
        public bool HasPrevious => Page > 1;
        /// <summary>
        /// Запрошенный размер страницы
        /// </summary>
        public int PageSize { get; set; }

    }
}
