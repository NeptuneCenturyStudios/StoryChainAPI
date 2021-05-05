using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class StoriesController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public StoriesController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
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
        [Route("mine")]
        public async Task<IActionResult> MyStories(int page, int take)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);


                // Get the stories that the user authored or helped author
                var stories = (from s in _db.Stories
                               where s.CreatedBy == user || s.Scenes.Any(sc => sc.Author == user)
                               select s);

                // Get the first scene the user contributed
                foreach (var story in stories)
                {
                    story.Scenes = _db.Scenes.Where((s) => s.Story.Id == story.Id && s.Author == user).ToList();

                }

                return Ok(stories);

            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}

