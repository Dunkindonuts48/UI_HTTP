using System;

namespace HTTP_NET_Project
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }

        public User(string username, string password, string salt)
        {
            Username = username;
            Password = password;
            Salt = salt;
        }

        public override string ToString()
        {
            return $"Username: {Username}\n" +
                   $"Password: {Password}\n" +
                   $"Salt: {Salt}\n"; 
        }
    }
}