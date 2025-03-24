namespace BugTrackingSystem.Models
{
    public class ProjectInvitation
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }             // Which project they're being invited to
        public required string InvitedUserEmail { get; set; }   // Email to invite
        public RoleType Role { get; set; }             // Role to assign after acceptance
        public required string InvitedByUserId { get; set; }    // Who sent the invite
        public bool IsAccepted { get; set; } = false;  // Status flag
    }
}