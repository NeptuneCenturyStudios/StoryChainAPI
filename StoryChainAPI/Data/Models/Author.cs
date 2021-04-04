using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.Data.Models
{
    public class Author
    {
        public long Id { get; set; }
        public IdentityUser User { get; set; }

    }
}
