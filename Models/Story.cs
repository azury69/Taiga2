using System.Diagnostics;
using System.Net.Mail;

namespace BugTrackingSystem.Models
{
    public class Story
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int? Points { get; set; }             
        public int ProjectId { get; set; }
        public string? AssignedToId { get; set; }    
        public int? SprintId { get; set; }
        public StoryStatus Status { get; set; } = StoryStatus.ToDo; // NEW FIELD, default


    }
}
