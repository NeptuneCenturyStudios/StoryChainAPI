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

        public StoryController(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        /// <summary>
        /// Creates a new story
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateStory(CreateStoryRequest request)
        {
            using (var transaction = _db.Database.BeginTransaction())
            using (_db)
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

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> MyStories(int page, int take)
        {
            var user = await _userManager.GetUserAsync(User);

            using (_db)
            {
                // Get the stories that the user authored or helped author
                var stories = (from s in _db.Stories
                               where s.CreatedBy == user
                               select s).ToArray();

                return Ok(stories);
            }
        }
    }
}
