using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.Data.Models
{
    public class Lock
    {
        public long Id { get; set; }
        public Story Story { get; set; }
        public IdentityUser User { get; set; }
        public DateTime LockedUntil { get; set; }


    }
}
