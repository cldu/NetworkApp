using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Network.API.Models
{
    public class User : IdentityUser<int>
    {
        public string Gender { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Nickname { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastActive { get; set; }

        public string Introduction { get; set; }

        public string Interests { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public ICollection<Photo> Photos { get; set; }

        public ICollection<Friend> Frienders { get; set; }

        public ICollection<Friend> Friendees { get; set; }

        public ICollection<UserRole> UserRoles { get; set; }

    }
}
