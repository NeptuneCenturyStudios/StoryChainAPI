using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.Data.Models
{
    public class Scene
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public IdentityUser Author { get; set; }
        public Story Story { get; set; }
        public List<Vote> Votes { get; set; }
        public DateTime WrittenOn { get; set; }

    }
}
