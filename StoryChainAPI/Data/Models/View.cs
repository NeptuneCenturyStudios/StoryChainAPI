using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.Data.Models
{
    public class View
    {
        public long Id { get; set; }
        [Required]
        public string IPAddress { get; set; }
        [Required]
        public DateTime ViewedOn { get; set; }
        public Story Story { get; set; }

    }
}
