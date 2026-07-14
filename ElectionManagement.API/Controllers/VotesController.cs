using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

[ApiController]
[Route("api/[controller]")]
public class VotesController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public VotesController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult GetVotes()
    {
        List<object> votes = new();

        string connectionString =
            _configuration.GetConnectionString("DefaultConnection");

        using SqlConnection con = new(connectionString);

        string query = @"
        SELECT
            v.VoteID,
            e.ElectionName,
            c.FullName,
            p.PartyName,
            vt.FullName AS VoterName,
            v.VoteDate
        FROM Votes v
        INNER JOIN Elections e
            ON v.ElectionID = e.ElectionID
        INNER JOIN Candidates c
            ON v.CandidateID = c.CandidateID
        INNER JOIN PoliticalParties p
            ON c.PartyID = p.PartyID
        INNER JOIN Voters vt
            ON v.VoterID = vt.VoterID
        ORDER BY v.VoteDate DESC";

        using SqlCommand cmd = new(query, con);

        con.Open();

        SqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            votes.Add(new
            {
                VoteID = Convert.ToInt32(reader["VoteID"]),
                ElectionName = reader["ElectionName"].ToString(),
                CandidateName = reader["FullName"].ToString(),
                PartyName = reader["PartyName"].ToString(),
                VoterName = reader["VoterName"].ToString(),
                VoteDate = Convert.ToDateTime(reader["VoteDate"])
            });
        }

        return Ok(votes);
    }
}