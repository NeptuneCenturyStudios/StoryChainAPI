using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StoryChainAPI.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StoryChainAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Story> Stories { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Scene> Scenes { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Lock> Locks { get; set; }
    }
}
