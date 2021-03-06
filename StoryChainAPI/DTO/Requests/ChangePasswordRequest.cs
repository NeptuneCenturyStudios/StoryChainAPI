using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.DTO.Requests
{
    public class ChangePasswordRequest
    {
        [Required]
        public string UserId { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
