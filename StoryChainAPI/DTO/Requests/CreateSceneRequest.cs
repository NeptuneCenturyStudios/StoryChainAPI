using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.DTO.Requests
{
    public class CreateSceneRequest
    {
        public long LockId { get; set; }
        [Required]
        public string Scene { get; set; }
    }
}
