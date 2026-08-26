using EventManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagement.Context.Interfaces
{
    public interface IRepository<T> where T: class
    {
        IEnumerable<T> GetAll();
        T? GetById(Guid id);
        void Create(T data);
        void Update(T data);
        void Delete(Guid id);
        bool IsExist(Guid id);
    }
}
