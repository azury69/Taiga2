using System.Security.Claims;
using BugTrackingSystem.Data;
using BugTrackingSystem.Dto;
using BugTrackingSystem.Models;
using BugTrackingSystem.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTrackingSystem.Controllers
{
    [ApiController]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ProjectAccessService _projectAccessService;

        public ProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ProjectAccessService projectAccessService)
        {
            _context = context;
            _userManager = userManager;
            _projectAccessService= projectAccessService;
        }
        [HttpPost("api/projects")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createProjectDto)
        {
            if (createProjectDto == null || string.IsNullOrEmpty(createProjectDto.Name) || string.IsNullOrEmpty(createProjectDto.Description))
            {
                return BadRequest("Project name and description are required.");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("You must be logged in to create a project.");
            }

            var project = new Project
            {
                Name = createProjectDto.Name,
                Description = createProjectDto.Description,
                ApplicationUserId = currentUser.Id,
            };

            try
            {
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                var projectMember = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = currentUser.Id,
                    Role = RoleType.ProductOwner
                };
                _context.ProjectMembers.Add(projectMember);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(CreateProject), new { id = project.Id }, project);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        } 
        [HttpGet("api/getallprojects")]
        public IActionResult GetAllProjects()
        {
            var currentUser = _userManager.GetUserAsync(User).Result;
            if (currentUser == null)
            {
                return Unauthorized("You must be logged in to view projects.");
            }

            // Fetch projects where the user is either the creator or a member (working on)
            var projects = _context.Projects
                .Where(p => p.ApplicationUserId == currentUser.Id || p.Members.Any(m => m.UserId == currentUser.Id))
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.ApplicationUserId,
                    IsCreator = p.ApplicationUserId == currentUser.Id // Flag to identify if the current user created the project
                })
                .ToList();

            return Ok(projects);
        }

        [HttpGet("api/project/{id}")]
        public IActionResult GetProjectById(int id)
        {
            var project = _context.Projects
                .FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }


        [HttpDelete("api/project/{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var member = await _projectAccessService.GetUserProjectMember(currentUser.Id, id);
            if (member == null || !_projectAccessService.CanManageProject(member.Role))
            {
                return Forbid("You don't have permission to delete this project.");
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Project deleted successfully." });
        }



    }
}


  
