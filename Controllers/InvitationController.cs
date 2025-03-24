using BugTrackingSystem.Data;
using BugTrackingSystem.Dto;
using BugTrackingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTrackingSystem.Controllers
{
    [ApiController]
    [Route("api/invitations")]
    [Authorize]
    public class InvitationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public InvitationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // SEND INVITE
        [HttpPost("send")]
        public async Task<IActionResult> SendInvitation([FromBody] InvitationsDto dto)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var project = await _context.Projects.FindAsync(dto.ProjectId);
            if (project == null) return NotFound("Project not found.");

            // Optional: Check if inviter has permission (ProductOwner/ProjectManager)
            var member = await _context.ProjectMembers.FirstOrDefaultAsync(pm => pm.ProjectId == dto.ProjectId && pm.UserId == currentUser.Id);
            if (member == null || (member.Role != RoleType.ProductOwner && member.Role != RoleType.ProjectManager))
                return Forbid("You don't have permission to invite.");

            var invitation = new ProjectInvitation
            {
                ProjectId = dto.ProjectId,
                InvitedUserEmail = dto.InvitedUserEmail,
                Role = dto.Role,
                InvitedByUserId = currentUser.Id
            };

            _context.ProjectInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Invitation sent." });
        }

        // GET PENDING INVITES FOR CURRENT USER
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingInvitations()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var invites = await _context.ProjectInvitations
                .Where(i => i.InvitedUserEmail == currentUser.Email && !i.IsAccepted)
                .ToListAsync();

            return Ok(invites);
        }

        [HttpPost("respond")]
        public async Task<IActionResult> RespondInvitation([FromBody] RespondInvitationDto dto)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            var invite = await _context.ProjectInvitations.FindAsync(dto.InvitationId);
            if (invite == null) return NotFound("Invitation not found.");
            if (invite.InvitedUserEmail != currentUser.Email) return Forbid("You can't respond to this invite.");

            if (dto.Accept)
            {
                invite.IsAccepted = true;

                var projectMember = new ProjectMember
                {
                    ProjectId = invite.ProjectId,
                    UserId = currentUser.Id,
                    Role = invite.Role
                };

                _context.ProjectMembers.Add(projectMember);
            }
            else
            {
                _context.ProjectInvitations.Remove(invite);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = dto.Accept ? "Invitation accepted." : "Invitation rejected." });
        }
    }
    }
