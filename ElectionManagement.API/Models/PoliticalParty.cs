namespace ElectionManagement.API.Models
{
    public class PoliticalParty
    {
        public int PartyID { get; set; }

        public string PartyName { get; set; } = string.Empty;

        public string LeaderName { get; set; } = string.Empty;

        public string? Logo { get; set; }
    }
}