namespace ElectionManagement.API.Models
{
    public class Election
    {
        public int ElectionID { get; set; }
        public string ElectionName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}