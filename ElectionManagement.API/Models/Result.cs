namespace ElectionManagement.API.Models
{
    public class Result
    {
        public string CandidateName { get; set; } = string.Empty;

        public string PartyName { get; set; } = string.Empty;

        public int TotalVotes { get; set; }
    }
}