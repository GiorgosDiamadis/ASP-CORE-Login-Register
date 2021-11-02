using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication.Models;
using WebApplication.Models.DataTransferObjects;

namespace WebApplication.Database.DatabaseAccessObjects.Interfaces
{
    public interface IDatabaseAccessObject<T> where T : DataTransferObjectBase
    {
        IEnumerable<T> GetAll(Predicate<T> condition = null);
        void CreateTableIfNotExists();
        Task<Messenger> Remove(string id);
        Task<Messenger> Insert(Dictionary<string,object> parameters);
        Task<Messenger> Get(string name = null, string id = null);
        Task<Messenger> Edit(Dictionary<string,KeyValuePair<object,Type>> edit,Dictionary<string,KeyValuePair<object,Type>> where);
    }
}