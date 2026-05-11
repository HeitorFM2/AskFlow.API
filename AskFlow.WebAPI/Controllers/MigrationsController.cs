using AskFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AskFlow.WebAPI.Controllers
{
    [ApiController]
    [Route("api/v2/[controller]")]
    public class MigrationsController(AppDbContext dbContext) : ControllerBase
    {
        private readonly AppDbContext _dbContext = dbContext;

        [HttpPost("apply")]
        public async Task<IActionResult> Apply()
        {
            var pending = (await _dbContext.Database.GetPendingMigrationsAsync()).ToList();

            if (pending.Count == 0)
                return Ok(new { message = "No pending migrations.", applied = Array.Empty<string>() });

            await _dbContext.Database.MigrateAsync();

            return Ok(new { message = $"{pending.Count} migration(s) applied successfully.", applied = pending });
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            var applied = (await _dbContext.Database.GetAppliedMigrationsAsync()).ToList();
            var pending = (await _dbContext.Database.GetPendingMigrationsAsync()).ToList();

            return Ok(new { applied, pending });
        }
    }
}
