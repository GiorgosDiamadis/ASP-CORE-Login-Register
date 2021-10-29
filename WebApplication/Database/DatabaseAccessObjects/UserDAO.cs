using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MySqlConnector;
using WebApplication.Database.DatabaseAccessObjects.Interfaces;
using WebApplication.Models;

namespace WebApplication.Database.DatabaseAccessObjects
{
    public class UserDao : IDao<User>
    {
        private readonly MySqlContext _mySqlContext;
        private readonly int NAME_ROLE_CHAR_LENGTH = 120;

        private readonly int PASSWORD_LENGTH = 64;

        public UserDao(MySqlContext mySqlContext)
        {
            this._mySqlContext = mySqlContext;
        }

        public IEnumerable<User> GetAll(Predicate<User> condition = null)
        {
            return null;
        }

        public async void CreateTableIfNotExists()
        {
            Console.WriteLine("Creating users table");
            MySqlConnection connection = _mySqlContext.GetConnection();
            await connection.OpenAsync();
            MySqlCommand mySqlCommand =
                new MySqlCommand(
                    "create table if not exists users(id varchar(120) not null, user_name varchar(120) not null,user_role varchar(120) not null, user_salt varchar(120) not null, user_hash varchar(120) not null ,primary key(id));",
                    connection);

            await mySqlCommand.ExecuteReaderAsync();
            await connection.CloseAsync();
        }

        public async Task<bool> Register(User user)
        {
            string[] hashSalt = HashPassword(user.Password);
            string userId = Guid.NewGuid().ToString();

            MySqlConnection connection = _mySqlContext.GetConnection();
            await connection.OpenAsync();
            MySqlCommand mySqlCommand =
                new MySqlCommand(
                    "insert into users(id,user_name,user_role,user_salt,user_hash) values(@ID,@NAME,@ROLE,@SALT,@HASH) ",
                    connection);
            mySqlCommand.Parameters.AddWithValue("@ID", userId);
            mySqlCommand.Parameters.AddWithValue("@NAME", user.Name);
            mySqlCommand.Parameters.AddWithValue("@ROLE", (int) user.Role);
            mySqlCommand.Parameters.AddWithValue("@SALT", hashSalt[1]);
            mySqlCommand.Parameters.AddWithValue("@HASH", hashSalt[0]);


            await mySqlCommand.ExecuteReaderAsync();
            await connection.CloseAsync();

            return true;
        }

        public async Task<User> LogIn(string name, string enteredPassword)
        {
            MySqlConnection connection = _mySqlContext.GetConnection();
            await connection.OpenAsync();
            MySqlCommand mySqlCommand =
                new MySqlCommand(
                    "select * from users where user_name=@NAME;",
                    connection);
            mySqlCommand.Parameters.AddWithValue("@NAME", name);

            using (MySqlDataReader reader = await mySqlCommand.ExecuteReaderAsync())
            {
                User dbUser = new User();
                string storedSalt = "";
                string storedHash = "";
                while (reader.Read())
                {
                    dbUser.Name = reader.GetString(reader.GetOrdinal("user_name"));
                    int enumPos = Int32.Parse(reader.GetString(reader.GetOrdinal("user_role")));
                    dbUser.Role = (Role) enumPos;
                    dbUser.Id = reader.GetString(reader.GetOrdinal("id"));


                    storedSalt = reader.GetString(reader.GetOrdinal("user_salt"));
                    storedHash = reader.GetString(reader.GetOrdinal("user_hash"));
                }

                await connection.CloseAsync();

                if (CheckValidPassword(enteredPassword, storedSalt, storedHash))
                {
                    
                    return dbUser;
                }
                else
                {
                    return null;
                }
            }
        }

        private bool CheckValidPassword(string enteredPassword, string storedSalt, string storedHash)
        {
            return storedHash == HashPassword(enteredPassword, storedSalt)[0];
        }


        private static string[] HashPassword(string password, string salt = null)
        {
            string[] hashSalt = new string[2];

            string passwordSalt = "";

            RNGCryptoServiceProvider rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            if (salt == null)
            {
                byte[] buff = new byte[12];
                rngCryptoServiceProvider.GetBytes(buff);
                passwordSalt = Convert.ToBase64String(buff);
                hashSalt[1] = passwordSalt;
            }
            else
            {
                passwordSalt = salt;
            }


            string passwordWithSalt = password + passwordSalt;

            HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider();
            byte[] bhash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(passwordWithSalt));

            string passwordHashed = Convert.ToBase64String(bhash);
            hashSalt[0] = passwordHashed;
            return hashSalt;
        }

        public bool Remove(User data)
        {
            return false;
        }


        public async Task<User> Search(string name)
        {
            MySqlConnection connection = _mySqlContext.GetConnection();
            await connection.OpenAsync();
            MySqlCommand mySqlCommand =
                new MySqlCommand(
                    "select * from users where user_name=@NAME;",
                    connection);
            mySqlCommand.Parameters.AddWithValue("@NAME", name);

            using (MySqlDataReader reader = await mySqlCommand.ExecuteReaderAsync())
            {
                User dbUser = new User();
                while (reader.Read())
                {
                    dbUser.Name = reader.GetString(reader.GetOrdinal("user_name"));
                    dbUser.Password = reader.GetString(reader.GetOrdinal("user_password"));
                    int enumPos = Int32.Parse(reader.GetString(reader.GetOrdinal("user_role")));
                    dbUser.Role = (Role) enumPos;
                    dbUser.Id = reader.GetString(reader.GetOrdinal("id"));
                }

                await connection.CloseAsync();

                return dbUser;
            }
        }

        public User Edit(User data)
        {
            throw new NotImplementedException();
        }
    }
}