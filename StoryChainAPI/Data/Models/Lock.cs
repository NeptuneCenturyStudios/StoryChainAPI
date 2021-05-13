using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StoryChainAPI.Data.Models
{
    public class Lock
    {
        public long Id { get; set; }
        [JsonIgnore]
        public Story Story { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime LockStart { get; set; }
        public DateTime LockEnd { get; set; }


    }
}
