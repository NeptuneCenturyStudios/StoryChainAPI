using StoryChainAPI.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.DTO.Results
{
    public class LockResult
    {
        public long StoryId { get; set; }
        public long LockId { get; set; }
        public string Title { get; set; }
        public Scene[] Scenes { get; set; }
        public DateTime LockEnd { get; set; }
    }
}
