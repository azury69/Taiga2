using BugTrackingSystem.Data;
using BugTrackingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTrackingSystem.Service
{
    public class ProjectAccessService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectAccessService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ProjectMember?> GetUserProjectMember(string userId, int projectId)
        {
            return await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public bool CanManageProject(RoleType role)
        {
            return role == RoleType.ProductOwner || role == RoleType.ProjectManager;
        }

        public bool CanManageTickets(RoleType role)
        {
            return role == RoleType.ProductOwner || role == RoleType.ProjectManager ||
                   role == RoleType.SoftwareEngineer || role == RoleType.QA;
        }

        public bool CanComment(RoleType role)
        {
            return true; 
        }
    }
}
