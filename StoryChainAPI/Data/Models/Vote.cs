using Microsoft.AspNetCore.Identity;
using StoryChainAPI.Data.Models;
using StoryChainAPI.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.Data.Models
{
    public class Vote
    {
        public long Id { get; set; }
        public VoteKind Kind { get; set; }
        public IdentityUser User { get; set; }
        public DateTime VotedOn { get; set; }
        public string IpAddress { get; set; }
        public Scene Scene { get; set; }
    }
}
