using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace WhyIDidntKnowThisGameEarlier.SessionLogic
{
    public class User
    {
        public readonly Guid ID;

        public string Name { get; set; }

        public string Password { get; }

        public Participants SessionRole;

        private int usersCount = 0;

        public Session CurrentSession { get; private set; }

        public User(string password) 
        {
            usersCount++;

            ID = Guid.NewGuid();
            Name = "user" + ID.ToString().Substring(0, 4) + usersCount;
            Password = password;
        }

        ~User()
        {
            usersCount--;
        }
    }
}
