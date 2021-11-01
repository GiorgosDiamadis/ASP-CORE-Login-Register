using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication.Models;
using WebApplication.Models.DataTransferObjects;
using WebApplication.Models.Interfaces;

namespace WebApplication.Database.DatabaseAccessObjects.Interfaces
{
    public interface IDao<T> where T : DataTransferObjectBase
    {
        IEnumerable<T> GetAll(Predicate<T> condition = null);
        void CreateTableIfNotExists();
        bool Remove(T obj);
        Task<Messenger> Register(Dictionary<string,object> parameters);
        Task<Messenger> Search(string name = null, string id = null);
        T Edit(T obj);
    }
}