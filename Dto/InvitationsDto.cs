using BugTrackingSystem.Models;

namespace BugTrackingSystem.Dto
{
    public class InvitationsDto
    {
        public int ProjectId { get; set; }
        public required string InvitedUserEmail { get; set; }
        public RoleType Role { get; set; }
    }
    public class RespondInvitationDto
    {
        public int InvitationId { get; set; }
        public bool Accept { get; set; }
    }
}