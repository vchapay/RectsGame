using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhyIDidntKnowThisGameEarlier.SessionLogic;

namespace WhyIDidntKnowThisGameEarlier
{
    public sealed class Server
    {
        private List<User> users;
        private List<Session> sessions;


        public Server() { }

        public Session CreateSession(User user)
        {
            throw new NotImplementedException();
        }

        public void AddToSession(User user)
        {

        }

        public bool RemoveSession(Session session) 
        {
            throw new NotImplementedException();
        }
    }
}
