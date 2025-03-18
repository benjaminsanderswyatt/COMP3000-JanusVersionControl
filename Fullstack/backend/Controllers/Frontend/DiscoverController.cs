using backend.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace backend.Controllers.Frontend
{

    [Route("api/web/[controller]")]
    [EnableCors("FrontendPolicy")]
    [ApiController]
    [EnableRateLimiting("FrontendRateLimit")]
    public class DiscoverController : ControllerBase
    {
        private readonly JanusDbContext _janusDbContext;
        private readonly IMemoryCache _memoryCache;

        public DiscoverController(JanusDbContext janusDbContext, IMemoryCache memoryCache)
        {
            _janusDbContext = janusDbContext;
            _memoryCache = memoryCache;
        }


        // Every week randomly select 30 public repos to display on the discover page
        // GET: api/web/discover/repositories
        [HttpGet("Repositories")]
        public async Task<IActionResult> GetDiscoverRepos([FromQuery] int page = 1)
        {
            const int pageSize = 5;

            // Calculate the weekly seed
            var now = DateTime.UtcNow;
            int weekNumber = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                now, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday); // Week num (1-53) changes every monday
            int seed = now.Year * 100 + weekNumber; // Creates seed e.g. 202526


            // Create a key using the seed
            string cacheKey = $"WeeklyRepos_{seed}";


            // Get the weekly chosen repo ids
            List<int>? weeklyRepoIds = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                int daysUntilNextMonday = (DayOfWeek.Monday - now.DayOfWeek + 7) % 7;
                var nextMonday = now.AddDays(daysUntilNextMonday).Date;
                entry.AbsoluteExpiration = nextMonday;

                var allPublicRepoIds = await _janusDbContext.Repositories
                    .Where(r => !r.IsPrivate)
                    .Select(r => r.RepoId)
                    .ToListAsync();

                Random random = new Random(seed);
                return allPublicRepoIds
                    .OrderBy(r => random.Next())
                    .Take(30)
                    .ToList();
            });



            // Get the repos using ids (make sure they are still public)
            var repos = await _janusDbContext.Repositories
                .Where(r => weeklyRepoIds.Contains(r.RepoId) && !r.IsPrivate)
                .Include(r => r.Owner)
                .Include(r => r.Branches)
                    .ThenInclude(b => b.Commits)
                .AsNoTracking()
                .ToListAsync();

            // Order acoring to the weekly order
            var repoDict = repos.ToDictionary(r => r.RepoId);
            var validRepositoriesOrdered = weeklyRepoIds
                .Select(id => repoDict.GetValueOrDefault(id))
                .Where(r => r != null)
                .ToList();


            // Pagination
            int totalCount = validRepositoriesOrdered.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (page < 1 || (page > totalPages && totalPages > 0))
                return BadRequest(new { message = "Invalid page number" });

            var paginatedRepos = validRepositoriesOrdered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();



            var result = new
            {
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount,
                Repositories = paginatedRepos.Select(r => new
                {
                    Id = r.RepoId,
                    Name = r.RepoName,
                    Description = r.RepoDescription,
                    LastUpdated = r.Branches
                        .SelectMany(b => b.Commits)
                        .OrderByDescending(c => c.CommittedAt)
                        .FirstOrDefault()?.CommittedAt,
                    Owner = new
                    {
                        r.Owner.UserId,
                        r.Owner.Username
                    }
                })
            };

            return Ok(result);

        }



    }
}
