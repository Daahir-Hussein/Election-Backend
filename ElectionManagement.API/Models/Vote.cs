public class Vote
{
    public int VoteID { get; set; }
    public int ElectionID { get; set; }
    public string ElectionName { get; set; }

    public int CandidateID { get; set; }
    public string CandidateName { get; set; }

    public string PartyName { get; set; }

    public int VoterID { get; set; }
    public string VoterName { get; set; }

    public DateTime VoteDate { get; set; }
}