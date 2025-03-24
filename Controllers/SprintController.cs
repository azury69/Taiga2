using BugTrackingSystem.Data;
using BugTrackingSystem.Dto;
using BugTrackingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTrackingSystem.Controllers
{
    [Authorize]
    [ApiController]
    public class SprintController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SprintController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create a new sprint
        [HttpPost("api/sprints")]
        public async Task<IActionResult> CreateSprint([FromBody] CreateSprintDto dto)
        {
            if (dto == null || dto.StartDate == DateTime.MinValue || dto.EndDate == DateTime.MinValue || dto.ProjectId <= 0)
            {
                return BadRequest("All fields are required.");
            }

            var sprint = new Sprint
            {
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                ProjectId = dto.ProjectId
            };

            try
            {
                _context.Sprints.Add(sprint);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetSprintById), new { id = sprint.Id }, sprint);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // Delete a sprint
        [HttpDelete("api/sprint/{id}")]
        public async Task<IActionResult> DeleteSprint(int id)
        {
            var sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null)
            {
                return NotFound();
            }

            _context.Sprints.Remove(sprint);
            await _context.SaveChangesAsync();

            return NoContent(); // Return status 204 (No Content) after deletion
        }

        // Get sprint by id
        [HttpGet("api/sprint/{id}")]
        public async Task<IActionResult> GetSprintById(int id)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Stories)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null)
            {
                return NotFound();
            }

            return Ok(sprint);
        }

        // Get sprints by project id
        [HttpGet("api/sprintby/{projectId}")]
        public async Task<IActionResult> GetSprintsByProject(int projectId)
        {
            var sprints = await _context.Sprints
                .Where(s => s.ProjectId == projectId)
                .ToListAsync();

            return Ok(sprints);
        }
        [HttpPost("api/sprint/{id}/start")]
        public async Task<IActionResult> StartSprint(int id)
        {
            var sprint = await _context.Sprints.FindAsync(id);
            if (sprint == null) return NotFound();

            sprint.IsStarted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Sprint started." });
        }
        [HttpPatch("api/story/{id}/status")]
        public async Task<IActionResult> UpdateStoryStatus(int id, [FromBody] UpdateStoryStatusDto statusdto)
        {
            var story = await _context.Stories.FindAsync(id);
            if (story == null) return NotFound();

            story.Status = statusdto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Story status updated." });
        }
        [HttpGet("api/sprint/{id}/details")]
        public async Task<IActionResult> GetSprintDetails(int id)
        {
            var sprint = await _context.Sprints
                .Include(s => s.Stories)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sprint == null) return NotFound();

            var grouped = sprint.Stories
                .GroupBy(s => s.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.ToList());

            return Ok(new
            {
                Sprint = sprint,
                StoriesGroupedByStatus = grouped
            });
        }

    }
}

