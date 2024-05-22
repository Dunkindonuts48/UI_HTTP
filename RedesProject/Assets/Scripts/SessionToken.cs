using System;

namespace HTTP_NET_Project
{
    public class SessionToken
    {
        public string Token { get; private set; }
        public string Username { get; private set; }
        public DateTime Expiry { get; private set; }

        public SessionToken(string username)
        {
            Token = Guid.NewGuid().ToString();
            Username = username;
            Expiry = DateTime.UtcNow.AddHours(1); //Token valid for one hour
        }

        public static SessionToken CreateAdminSessionToken()
        {
            SessionToken newSessionToken = new SessionToken("admin"); 
            newSessionToken.Token = "admin_token";
            newSessionToken.Username = "admin";
            newSessionToken.Expiry = DateTime.MaxValue;
            return newSessionToken; 
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow > Expiry;
        }
    }
}