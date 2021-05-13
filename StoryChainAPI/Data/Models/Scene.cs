using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StoryChainAPI.Data.Models
{
    public class Scene
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public virtual ApplicationUser Author { get; set; }
        [JsonIgnore]
        public virtual Story Story { get; set; }
        public virtual List<Vote> Votes { get; set; }
        public virtual DateTime StartedOn { get; set; }
        public virtual DateTime? FinishedOn { get; set; }

    }
}
