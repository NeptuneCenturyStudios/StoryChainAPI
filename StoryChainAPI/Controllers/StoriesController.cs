using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryChainAPI.Data;
using StoryChainAPI.Data.Models;
using StoryChainAPI.DTO.Requests;
using StoryChainAPI.DTO.Results;
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
                    _db.Stories.Add(story);

                    // Add the genres
                    var genres = _db.Genres.Where(g => request.GenreIds.Contains(g.Id));
                    story.Genres = genres.ToList();

                    // Add the first scene
                    var scene = new Scene()
                    {
                        Text = request.FirstScene,
                        Story = story,
                        StartedOn = DateTime.UtcNow,
                        FinishedOn = DateTime.UtcNow,
                        Author = user
                    };
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
        [Route("lock")]
        public async Task<IActionResult> LockStory()
        {
            var user = await _userManager.GetUserAsync(User);

            // Does the user already have a locked story? Return the first locked story
            // that the user has. Perhaps it was an abandoned session?
            var userLockedStory = await (from s in _db.Stories.Include(x => x.Scenes).Include(x => x.Locks)
                                         where s.Locks.Any(l => l.User == user && l.LockEnd >= DateTime.UtcNow)
                                         select new LockResult()
                                         {
                                             StoryId = s.Id,
                                             Title = s.Title,
                                             Scenes = new[] { s.Scenes.OrderByDescending(sc => sc.FinishedOn).FirstOrDefault() },
                                             LockId = s.Locks.Single().Id,
                                             LockEnd = new DateTime(s.Locks.First().LockEnd.Ticks, DateTimeKind.Utc),
                                         }).FirstOrDefaultAsync();

            // Does the user have a locked story?
            if (userLockedStory != null)
            {
                return Ok(userLockedStory);
            }


            // Get all expired locks and delete them
            var expiredLocks = from l in _db.Locks
                               where l.LockEnd < DateTime.UtcNow
                               select l;

            _db.Locks.RemoveRange(expiredLocks);
            await _db.SaveChangesAsync();

            // Lock a random story that isn't locked or completed yet. Also, a story can only be locked
            // if the current user is not the last user to have written a scene.
            var storyToLock = await (from s in _db.Stories.Include(x => x.Scenes).Include(x => x.Locks)
                                     orderby Guid.NewGuid()
                                     where s.FinishedOn == null
                                     && !s.Locks.Any(l => l.Story.Id == s.Id)
                                     && s.Scenes.OrderByDescending(sc => sc.FinishedOn).Select(sc => sc.Author).FirstOrDefault() != user
                                     select new
                                     {
                                         Story = s,
                                         Scenes = new[] { s.Scenes.OrderByDescending(sc => sc.FinishedOn).FirstOrDefault() }
                                     }).FirstOrDefaultAsync();

            if (storyToLock != null)
            {
                var lockStart = DateTime.UtcNow;
                // Create a lock
                var storyLock = new Lock()
                {
                    LockStart = lockStart,
                    LockEnd = lockStart.AddSeconds(storyToLock.Story.SceneTimeLimitInSeconds),
                    Story = storyToLock.Story,
                    User = user
                };

                // Save the lock
                _db.Locks.Add(storyLock);
                await _db.SaveChangesAsync();

                // Return lock
                return Ok(new LockResult()
                {
                    StoryId = storyToLock.Story.Id,
                    Title = storyToLock.Story.Title,
                    Scenes = storyToLock.Scenes,
                    LockId = storyLock.Id,
                    LockEnd = storyLock.LockEnd
                });
            }
            else
            {
                // Nothing to play because there are no available stories to write
                return NoContent();
            }


        }

        [HttpGet]
        [Route("unlock")]
        public async Task<IActionResult> UnlockStory()
        {
            var user = await _userManager.GetUserAsync(User);

            // Get all for the user and release them
            var expiredLocks = from l in _db.Locks
                               where l.User == user
                               select l;

            _db.Locks.RemoveRange(expiredLocks);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("scene")]
        public async Task<IActionResult> CreateScene(CreateSceneRequest request)
        {
            var sceneId = 0L;
            using (var transaction = _db.Database.BeginTransaction())
            using (_db)
            {
                var user = await _userManager.GetUserAsync(User);

                try
                {

                    // Look up the lock. User can have only one lock at a time.
                    var lockedStory = await (from l in _db.Locks.Include(x => x.Story)
                                             where l.Id == request.LockId && l.LockEnd <= DateTime.UtcNow && l.User == user
                                             select l).SingleOrDefaultAsync();
                    if (lockedStory != null)
                    {
                        // Add the first scene
                        var scene = new Scene()
                        {
                            Text = request.Scene,
                            Story = lockedStory.Story,
                            StartedOn = lockedStory.LockStart,
                            FinishedOn = DateTime.UtcNow,
                            Author = user
                        };
                        _db.Scenes.Add(scene);

                        // Remove the lock
                        _db.Locks.Remove(lockedStory);

                        // Save the changes and commit the transaction
                        await _db.SaveChangesAsync();
                        transaction.Commit();

                        // Get the scene id
                        sceneId = scene.Id;
                    }
                    else
                    {
                        ModelState.AddModelError("", "User must have exactly one lock in order to create a scene for a story");
                        return BadRequest(ModelState);
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();

                    throw;
                }
            }


            return Created("", sceneId);
        }


        [HttpGet]
        [Route("mine")]
        public async Task<IActionResult> MyStories(int page, int take)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);


                // Get the stories that the user authored or helped author
                var stories = (from s in _db.Stories.Include(x => x.CreatedBy)
                               where s.CreatedBy == user || s.Scenes.Any(sc => sc.Author == user)
                               select new
                               {
                                   Title = s.Title,
                                   CreatedBy = s.CreatedBy,
                                   Scene = s.Scenes.OrderByDescending(sc => sc.FinishedOn).FirstOrDefault(sc => sc.Author == user),
                                   MaxScenes = s.MaxScenes,
                                   TotalScenes = s.Scenes.Count(),
                                   Progress = s.Scenes.Count() / (decimal)s.MaxScenes * 100
                               });

                return Ok(stories);

            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("{storyId}")]
        public async Task<IActionResult> GetStory(long storyId)
        {

            // Get the specified story IF the story is complete. All other cases the user is not
            // allowed to view the story.
            var story = await (from s in _db.Stories.Include(x => x.Scenes).Include(x => x.Genres)
                               where s.Id == storyId && s.FinishedOn != null
                               select new
                               {
                                   s.Title,
                                   Scenes = from sc in s.Scenes
                                            select new
                                            {
                                                sc.Text,
                                                AuthorFirstName = sc.Author.FirstName,
                                                AuthorLastName = sc.Author.LastName,
                                                sc.StartedOn,
                                                sc.FinishedOn
                                            },
                                   AuthorFirstName = s.CreatedBy.FirstName,
                                   AuthorLastName = s.CreatedBy.LastName,
                                   s.StartedOn,
                                   s.FinishedOn,
                                   s.Genres
                               }).FirstOrDefaultAsync();

            if (story != null)
            {
                return Ok(story);
            }
            else
            {
                return Forbid();
            }
        }

        [HttpGet]
        [Route("{storyId}/scene/{sceneId}")]
        public async Task<IActionResult> GetScene(long storyId, int sceneId)
        {
            var user = await _userManager.GetUserAsync(User);
            // Get the specified scene from the specified story IF the story is complete
            // OR if the user is the author of the scene. All other cases the user is not
            // allowed to view the scene.
            var scene = await (from sc in _db.Scenes
                               where sc.Story.Id == storyId && (sc.FinishedOn != null && sc.Id == sceneId || sc.FinishedOn == null && sc.Author == user)
                               select new
                               {
                                   sc.Story.Title,
                                   sc.Text,
                                   sc.FinishedOn,
                                   sc.Author.FirstName,
                                   sc.Author.LastName
                               }).FirstOrDefaultAsync();

            if (scene != null)
            {
                return Ok(scene);
            }
            else
            {
                return Forbid();
            }
        }
    }
}

