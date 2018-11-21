using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Network.API.Models
{
    public class Friend
    {
        public int FrienderId { get; set; }

        public int FriendeeId { get; set; }

        public User Friender { get; set; }

        public User Friendee { get; set; }
    }
}
