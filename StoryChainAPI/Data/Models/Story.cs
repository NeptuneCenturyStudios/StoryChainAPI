
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.Data.Models
{
    public class Story
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public int Rating { get; set; }
        public List<Tag> Tags { get; set; }

    }
}
