using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.DTO.Requests
{
    public class CreateStoryRequest
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string FirstScene { get; set; }
        public bool OnlyShowPreviousScene { get; set; }
        [Range(5, 25)]
        public int NumberOfScenes { get; set; }
    }
}
