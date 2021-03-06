using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StoryChainAPI.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace StoryChainAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Seed data we need
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Genre>().HasData(new Genre { Id = 1, Name = "Fantasy" });
            builder.Entity<Genre>().HasData(new Genre { Id = 2, Name = "Science Fiction" });
            builder.Entity<Genre>().HasData(new Genre { Id = 3, Name = "Mystery" });
            builder.Entity<Genre>().HasData(new Genre { Id = 4, Name = "Thriller" });
            builder.Entity<Genre>().HasData(new Genre { Id = 5, Name = "Romance" });
            builder.Entity<Genre>().HasData(new Genre { Id = 6, Name = "Western" });
            builder.Entity<Genre>().HasData(new Genre { Id = 7, Name = "Dystopian" });
            builder.Entity<Genre>().HasData(new Genre { Id = 8, Name = "Contemporary" });
            builder.Entity<Genre>().HasData(new Genre { Id = 9, Name = "Historical Fiction" });
            builder.Entity<Genre>().HasData(new Genre { Id = 10, Name = "Action and Adventure" });
            builder.Entity<Genre>().HasData(new Genre { Id = 11, Name = "Horror" });

            builder.Entity<Audience>().HasData(new Audience { Id = 1, Name = "Children" });
            builder.Entity<Audience>().HasData(new Audience { Id = 2, Name = "Middle Grade" });
            builder.Entity<Audience>().HasData(new Audience { Id = 3, Name = "Young Adult" });
            builder.Entity<Audience>().HasData(new Audience { Id = 4, Name = "Adult" });
        }

        public virtual DbSet<Story> Stories { get; set; }
        public virtual DbSet<Scene> Scenes { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        public virtual DbSet<Genre> Genres { get; set; }
        public virtual DbSet<Audience> Audiences { get; set; }
        public virtual DbSet<Vote> Votes { get; set; }
        public virtual DbSet<Lock> Locks { get; set; }
        public virtual DbSet<View> Views { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
