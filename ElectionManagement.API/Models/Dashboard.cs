namespace ElectionManagement.API.Models
{
    public class Dashboard
    {
        public int TotalElections { get; set; }

        public int TotalParties { get; set; }

        public int TotalCandidates { get; set; }

        public int TotalVoters { get; set; }

        public int TotalVotes { get; set; }
    }
}