using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StoryChainAPI.Data;
using StoryChainAPI.Data.Models;
using StoryChainAPI.DTO.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StoryChainAPI.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class StoryController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;
        
        public StoryController(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _db = dbContext;
        }

        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateStory(CreateStoryRequest request)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                var user = await _userManager.GetUserAsync(User);

                try
                {

                    // Look up the audience
                    var audience = await _db.Audiences.FindAsync(4);
                    var genre = await _db.Genres.FindAsync(2);

                    // Create a story
                    var story = new Story
                    {
                        Title = request.Title,
                        SceneTimeLimitInSeconds = 600,
                        MaxScenes = request.NumberOfScenes,
                        ShowAllPreviousScenes = !request.OnlyShowPreviousScene,
                        Audience = audience,
                        StartedOn = DateTime.UtcNow,
                        CreatedBy = user
                    };

                    // Add the genres
                    story.Genres = new List<Genre>
                    {
                        genre
                    };

                    // Add the first scene
                    var scene = new Scene()
                    {
                        Text = request.FirstScene,
                        Story = story,
                        WrittenOn = DateTime.UtcNow,
                        Author = user
                    };

                    // Add story
                    _db.Stories.Add(story);

                    // Add scene
                    _db.Scenes.Add(scene);


                    await _db.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();

                    throw;
                }
            }






            return Created("", "");
        }
    }
}
