using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using MySqlConnector;
using WebApplication.Database.DatabaseAccessObjects.Interfaces;
using WebApplication.Models;
using RestSharp;
using RestSharp.Authenticators;

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
                    @"create table if not exists users(
                    id varchar(120) unique not null,
                    has_validated int not null,
                     user_name varchar(120) unique not null,
                    user_role varchar(120) not null,
                    user_phone varchar(32) not null,
                    user_email varchar(64) not null ,
                    user_salt varchar(120) not null, 
                    user_hash varchar(120) not null ,
                    primary key(id));",
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
                    @"insert into users(id,has_validated,user_name,user_role,user_phone,user_email,user_salt,user_hash)
                     values(@ID,@HASVALIDATED,@NAME,@ROLE,@PHONE,@EMAIL,@SALT,@HASH) ",
                    connection);

            mySqlCommand.Parameters.AddWithValue("@ID", userId);
            mySqlCommand.Parameters.AddWithValue("@HASVALIDATED", 0);
            mySqlCommand.Parameters.AddWithValue("@NAME", user.Name);
            mySqlCommand.Parameters.AddWithValue("@ROLE", (int) user.Role);
            mySqlCommand.Parameters.AddWithValue("@PHONE", user.PhoneNumber);
            mySqlCommand.Parameters.AddWithValue("@EMAIL", user.Email);
            mySqlCommand.Parameters.AddWithValue("@SALT", hashSalt[1]);
            mySqlCommand.Parameters.AddWithValue("@HASH", hashSalt[0]);


            try
            {
                await mySqlCommand.ExecuteReaderAsync();
                await connection.CloseAsync();
                user.Id = userId;
                SendConfirmationEmail(user);
                return true;
            }
            catch (Exception e)
            {
                await connection.CloseAsync();
                return false;
            }
        }

        public async Task<bool> ConfirmEmail(string token)
        {
            MySqlConnection connection = _mySqlContext.GetConnection();
            await connection.OpenAsync();
            try
            {
                MySqlCommand mySqlCommand =
                    new MySqlCommand(
                        @"update users set has_validated=1 where id=@ID",
                        connection);
                mySqlCommand.Parameters.AddWithValue("@ID", token);
                await mySqlCommand.ExecuteReaderAsync();
                await connection.CloseAsync();
                return true;
            }
            catch (Exception e)
            {
                await connection.CloseAsync();
                Console.WriteLine(e);
                return false;
            }
        }

        private IRestResponse SendConfirmationEmail(User user)
        {
            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.eu.mailgun.net/v3");
            client.Authenticator =
                new HttpBasicAuthenticator("api",
                    "key-3fd13d45e6a1d3243db9bbd88bf69780");
            RestRequest request = new RestRequest();
            request.AddParameter("domain", "mg.diamadisgiorgos.com", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Diamadis Giorgos | BugTracker <mailgun@mg.diamadisgiorgos.com>");

            request.AddParameter("to", user.Email);

            request.AddParameter("subject", "Hello Diamadis Giorgos");
            request.AddParameter("template", "confirmationmail");
            var link = "http://localhost:5000/confirmEmail?token=" + user.Id;
            request.AddParameter("v:link", link);
            request.Method = Method.POST;
            return client.Execute(request);
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
                int hasValidated = 0;
                Console.WriteLine(dbUser);
                while (reader.Read())
                {
                    dbUser.Name = reader.GetString(reader.GetOrdinal("user_name"));
                    int enumPos = Int32.Parse(reader.GetString(reader.GetOrdinal("user_role")));
                    dbUser.Role = (Role) enumPos;
                    dbUser.Id = reader.GetString(reader.GetOrdinal("id"));
                    dbUser.PhoneNumber = reader.GetString(reader.GetOrdinal("user_phone"));
                    dbUser.Email = reader.GetString(reader.GetOrdinal("user_email"));

                    hasValidated = reader.GetInt32(reader.GetOrdinal("has_validated"));

                    storedSalt = reader.GetString(reader.GetOrdinal("user_salt"));
                    storedHash = reader.GetString(reader.GetOrdinal("user_hash"));
                }

                await connection.CloseAsync();

                if (CheckValidPassword(enteredPassword, storedSalt, storedHash) && hasValidated == 1)
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


        public async Task<User> Search(string name = null, string id = null)
        {
            MySqlConnection connection = _mySqlContext.GetConnection();
            await connection.OpenAsync();
            MySqlCommand mySqlCommand;
            if (id == null)
            {
                mySqlCommand =
                    new MySqlCommand(
                        "select * from users where user_name=@NAME;",
                        connection);
                mySqlCommand.Parameters.AddWithValue("@NAME", name);
            }
            else
            {
                mySqlCommand =
                    new MySqlCommand(
                        "select * from users where id=@ID;",
                        connection);
                mySqlCommand.Parameters.AddWithValue("@ID", id);
            }

            using (MySqlDataReader reader = await mySqlCommand.ExecuteReaderAsync())
            {
                User dbUser = new User();
                while (reader.Read())
                {
                    dbUser.Name = reader.GetString(reader.GetOrdinal("user_name"));
                    dbUser.Password = reader.GetString(reader.GetOrdinal("user_password"));
                    dbUser.PhoneNumber = reader.GetString(reader.GetOrdinal("user_phone"));
                    dbUser.Email = reader.GetString(reader.GetOrdinal("user_email"));
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
            return null;
        }
    }
}