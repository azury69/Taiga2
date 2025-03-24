namespace BugTrackingSystem.Models
{
    public class Sprint
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ProjectId { get; set; }
        public List<Story> Stories { get; set; } = new List<Story>();
        public bool IsStarted { get; set; } = false; // NEW FIELD

    }
}
