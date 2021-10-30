using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication.Database.DatabaseAccessObjects.Interfaces;
using WebApplication.Models;
using WebApplication.Models.DataTransferObjects;

namespace WebApplication.Database.DatabaseAccessObjects
{
    public class BugDao : IDao<DataTransferObjectBase>
    {
        private readonly MySqlContext _mySqlContext;

        public BugDao(MySqlContext mySqlContext)
        {
            this._mySqlContext = mySqlContext;
        }

        public IEnumerable<DataTransferObjectBase> GetAll(Predicate<DataTransferObjectBase> condition = null)
        {
            throw new NotImplementedException();
        }

        public void CreateTableIfNotExists()
        {
            return;
        }

        public bool Remove(DataTransferObjectBase data)
        {
            throw new NotImplementedException();
        }

        public Task<Messenger> Register(DataTransferObjectBase user)
        {
            throw new NotImplementedException();
        }

        public Task<Messenger> Insert(DataTransferObjectBase obj)
        {
            throw new NotImplementedException();
        }

        public async Task<DataTransferObjectBase> Search(string name = null, string id = null)
        {
            throw new NotImplementedException();
        }

        public DataTransferObjectBase Edit(DataTransferObjectBase data)
        {
            throw new NotImplementedException();
        }
    }
}