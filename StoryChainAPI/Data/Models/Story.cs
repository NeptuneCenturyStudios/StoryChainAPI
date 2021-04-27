
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.Data.Models
{
    public class Story
    {
        public long Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public bool ShowAllPreviousScenes { get; set; }
        [Required]
        public int MaxScenes { get; set; }
        /// <summary>
        /// Gets or sets how long the author has to write their scene. This is also the length of the lock.
        /// </summary>
        [Required]
        public int SceneTimeLimitInSeconds { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Genre> Genres { get; set; }
        public Audience Audience { get; set; }
        [Required]
        public DateTime StartedOn { get; set; }
        public DateTime? FinishedOn { get; set; }
        public IdentityUser CreatedBy { get; set; }

    }
}
