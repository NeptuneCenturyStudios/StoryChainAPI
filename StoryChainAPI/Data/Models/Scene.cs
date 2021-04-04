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
        public IdentityUser Author { get; set; }
        public string SceneText { get; set; }

    }
}
