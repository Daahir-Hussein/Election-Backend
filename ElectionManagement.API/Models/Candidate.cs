namespace ElectionManagement.API.Models
{
    public class Candidate
    {
        public int CandidateID { get; set; }

        public int ElectionID { get; set; }

        public int PartyID { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public string? Photo { get; set; }

        public string? Biography { get; set; }
    }
}