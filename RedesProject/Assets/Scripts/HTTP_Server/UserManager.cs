using System;
using System.Collections.Generic;

namespace HTTP_NET_Project
{
    public class UserManager
    {
        private Dictionary<string, User> _users = new Dictionary<string, User>();
        private Dictionary<string, SessionToken> _sessions = new Dictionary<string, SessionToken>();

        public bool CreateUser(string username, string password)
        {
            if (_users.ContainsKey(username))
            {
                return false; //user already exists
            }

            string salt = Guid.NewGuid().ToString().Substring(0, 6); //creates a random 6 character long string to encrypt the password with
            string encryptedPassword = mUtils.EncryptPassword(password, salt);
            _users.Add(username, new User(username, encryptedPassword, salt));
            return true;
        }

        public bool UpdateUser(string username, string newPassword)
        {
            if (!_users.ContainsKey(username))
            {
                return false; //user does not exist
            }

            string newEncryptedPassword = mUtils.EncryptPassword(newPassword, _users[username].Salt);
            _users[username].Password = newEncryptedPassword;
            return true;
        }

        public bool DeleteUser(string username)
        {
            return _users.Remove(username); //returns true if user was removed, false if user does not exist because it cant find it
        }

        public User GetUser(string username)
        {
            _users.TryGetValue(username, out var user); //assigns to the variable "user" the user if it exists, otherwise it assigns null
            return user;
        }

        public bool ValidateUser(string username, string password)
        {
            if (!_users.ContainsKey(username))
            {
                return false; //user does not exist
            }

            string decryptedPassword = mUtils.DecryptPassword(_users[username].Password);
            return decryptedPassword == password;
        }

        public SessionToken CreateSession (string username)
        {
            SessionToken token = new SessionToken(username);
            _sessions[token.Token] = token;
            return token;
        }

        public bool ValidateSession (string token)
        {
            if (_sessions.ContainsKey(token))
            {
                if (_sessions[token].IsExpired())
                {
                    _sessions.Remove(token);
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates and add the admin user
        /// </summary>
        public void CreateAdminUser()
        {
            CreateUser("admin", "admin"); 
            SessionToken adminSessionToken = SessionToken.CreateAdminSessionToken();
            _sessions.Add("admin_token", adminSessionToken);
        }

        public Dictionary<string, User> GetUsersDictionary()
        {
            return _users; 
        }
    }
}