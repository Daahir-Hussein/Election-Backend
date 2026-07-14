namespace ElectionManagement.API.Models
{
    public class VoteRequest
    {
        public int VoterID { get; set; }
        public int ElectionID { get; set; }
        public int CandidateID { get; set; }
    }
}