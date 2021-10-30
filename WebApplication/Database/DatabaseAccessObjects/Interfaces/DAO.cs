using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication.Models.Interfaces;

namespace WebApplication.Database.DatabaseAccessObjects.Interfaces
{
    public interface IDao<T> where T : IDatabaseModel
    {
        IEnumerable<T> GetAll(Predicate<T> condition = null);
        void CreateTableIfNotExists();
        bool Remove(T data);
        Task<bool> Register(T user);
        Task<T> Search(string name = null, string id = null);
        T Edit(T data);
    }
}