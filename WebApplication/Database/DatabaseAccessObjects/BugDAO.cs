using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication.Database.DatabaseAccessObjects.Interfaces;
using WebApplication.Models;

namespace WebApplication.Database.DatabaseAccessObjects
{
    public class BugDAO : IDao<Bug>
    {
        private readonly MySqlContext _mySqlContext;

        public BugDAO(MySqlContext mySqlContext)
        {
            this._mySqlContext = mySqlContext;
        }

        public IEnumerable<Bug> GetAll(Predicate<Bug> condition = null)
        {
            throw new NotImplementedException();
        }

        public void CreateTableIfNotExists()
        {
            return;
        }

        public bool Remove(Bug data)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Register(Bug user)
        {
            throw new NotImplementedException();
        }

        public async Task<Bug> Search(string name = null, string id = null)
        {
            throw new NotImplementedException();
        }

        public Bug Edit(Bug data)
        {
            throw new NotImplementedException();
        }
    }
}