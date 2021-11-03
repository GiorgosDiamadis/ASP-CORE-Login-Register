using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using WebApplication.Database.DatabaseAccessObjects.Interfaces;
using WebApplication.Models;
using WebApplication.Models.DataTransferObjects;
using WebApplication.Models.Interfaces;

namespace WebApplication.Database.DatabaseAccessObjects
{
    public class PasswordRecoveryDbAccess : IDatabaseAccessObject<DataTransferObjectBase>
    {
        private readonly MySqlContext _mySqlContext;
        private IDatabaseAccessObject<DataTransferObjectBase> _databaseAccessObjectImplementation;

        public IEnumerable<DataTransferObjectBase> GetAll(Predicate<DataTransferObjectBase> condition = null)
        {
            return _databaseAccessObjectImplementation.GetAll(condition);
        }

        public PasswordRecoveryDbAccess(MySqlContext mySqlContext)
        {
            _mySqlContext = mySqlContext;
        }

        public async void CreateTableIfNotExists()
        {
            Console.WriteLine("Creating password recoveries table");
            MySqlConnection connection = _mySqlContext.GetConnection();
            await connection.OpenAsync();
            MySqlCommand mySqlCommand =
                new MySqlCommand(
                    @"create table if not exists password_recoveries(
                    id varchar(120) unique not null,
                    token varchar(120) not null,
                     user_name varchar(120) unique not null,
                     date_created varchar(120) not null ,
                     expires_in varchar(120) not null ,
                    primary key(id));",
                    connection);

            await mySqlCommand.ExecuteReaderAsync();
            await connection.CloseAsync();
        }

        public async Task<Messenger> Remove(string userName)
        {
            MySqlConnection connection = _mySqlContext.GetConnection();
            await connection.OpenAsync();
            MySqlCommand mySqlCommand =
                new MySqlCommand(
                    @"delete from password_recoveries where user_name=@ID;",
                    connection);

            mySqlCommand.Parameters.AddWithValue("@ID", userName);

            try
            {
                await mySqlCommand.ExecuteReaderAsync();
                await connection.CloseAsync();
                return new Messenger("", false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await connection.CloseAsync();
                return new Messenger("", true);
            }
        }

        public async Task<Messenger> Insert(Dictionary<string, object> parameters)
        {
            MySqlConnection connection = _mySqlContext.GetConnection();
            await connection.OpenAsync();
            string id = Guid.NewGuid().ToString();
            Messenger result;


            try
            {
                MySqlCommand mySqlCommand =
                    new MySqlCommand(
                        @"insert into password_recoveries(id,token,date_created,expires_in,user_name) values(@ID,@TOKEN,@CREATED,@EXPIRES,@USERNAME)",
                        connection);


                DateTime now = DateTime.Now;


                mySqlCommand.Parameters.AddWithValue("@ID", id);
                mySqlCommand.Parameters.AddWithValue("@TOKEN", parameters["token"].ToString());
                mySqlCommand.Parameters.AddWithValue("@CREATED", now);
                mySqlCommand.Parameters.AddWithValue("@EXPIRES", now.AddMinutes(30));
                mySqlCommand.Parameters.AddWithValue("@USERNAME", parameters["user_name"].ToString());
                MySqlDataReader reader = await mySqlCommand.ExecuteReaderAsync();

                if (reader.RecordsAffected > 0)
                {
                    result = new Messenger("We have sent you a password recovery link to your email. This link will expire in 3 minutes!",
                        false);
                }
                else
                {
                    result = new Messenger("Something went wrong during the password recovery process!",
                        true);
                }

                await connection.CloseAsync();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                if (e.Message.Contains("Duplicate entry"))
                {
                    Dictionary<string, KeyValuePair<object, Type>> edit =
                        new Dictionary<string, KeyValuePair<object, Type>>();

                    Dictionary<string, KeyValuePair<object, Type>> where =
                        new Dictionary<string, KeyValuePair<object, Type>>();

                    edit["token"] =
                        new KeyValuePair<object, Type>(new string(parameters["token"].ToString()), typeof(string));

                    where["user_name"] = new KeyValuePair<object, Type>(new string(parameters["user_name"].ToString()),
                        typeof(string));

                    await Edit(edit, where);

                    result = new Messenger("We have sent you a password recovery link to your email.",
                        false);
                    await connection.CloseAsync();
                    return result;
                }

                result = new Messenger("Something went wrong during the password recovery process!",
                    true);
                await connection.CloseAsync();
                return result;
            }
        }

        public async Task<Messenger> Get(string name = null, string id = null)
        {
            MySqlConnection connection = _mySqlContext.GetConnection();
            await connection.OpenAsync();

            MySqlCommand mySqlCommand =
                new MySqlCommand(@"select * from password_recoveries where token=@ID", connection);
            mySqlCommand.Parameters.AddWithValue("@ID", id);
            PasswordRecoveryEntry passwordRecoveryEntry = new PasswordRecoveryEntry();
            Messenger result;
            try
            {
                MySqlDataReader reader = await mySqlCommand.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        passwordRecoveryEntry.Username = reader.GetString(reader.GetOrdinal("user_name"));
                        passwordRecoveryEntry.Id = reader.GetString(reader.GetOrdinal("id"));
                        passwordRecoveryEntry.Token = reader.GetString(reader.GetOrdinal("token"));
                        passwordRecoveryEntry.DateCreate = reader.GetString(reader.GetOrdinal("date_created"));
                        passwordRecoveryEntry.ExpirationDate = reader.GetString(reader.GetOrdinal("expires_in"));
                    }

                    result = new Messenger("", false);
                    result.SetData(passwordRecoveryEntry);
                }
                else
                {
                    result = new Messenger("Invalid token.", true);
                }

                await connection.CloseAsync();
                return result;
            }
            catch (Exception e)
            {
                result = new Messenger("Invalid token.", true);
                await connection.CloseAsync();
                return result;
            }
        }

        public async Task<Messenger> Edit(Dictionary<string, KeyValuePair<object, Type>> edit,
            Dictionary<string, KeyValuePair<object, Type>> @where)
        {
            string query = "update password_recoveries set ";
            foreach (var kv in edit)
            {
                query += kv.Key + "=";
                if (kv.Value.Value == typeof(string))
                {
                    query += $"'{kv.Value.Key}',";
                }
                else
                {
                    query += (int) kv.Value.Key + ",";
                }
            }

            query = query.Remove(query.Length - 1, 1);
            query += " where ";

            foreach (var kv in where)
            {
                query += kv.Key + "=";
                if (kv.Value.Value == typeof(string))
                {
                    query += $"'{kv.Value.Key}' and ";
                }
                else
                {
                    query += (int) kv.Value.Key + " and ";
                }
            }

            query = query.Remove(query.Length - 5, 4);

            MySqlConnection connection = _mySqlContext.GetConnection();
            await connection.OpenAsync();
            MySqlCommand mySqlCommand = new MySqlCommand(query, connection);


            try
            {
                await mySqlCommand.ExecuteReaderAsync();
                await connection.CloseAsync();

                return new Messenger("", false);
            }
            catch (Exception e)
            {
                await connection.CloseAsync();

                return new Messenger("", true);
            }
        }
    }
}