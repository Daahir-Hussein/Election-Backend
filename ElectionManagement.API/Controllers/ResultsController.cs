using ElectionManagement.API.Data;
using ElectionManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ElectionManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        [HttpGet("{electionId}")]
        public IActionResult GetResults(int electionId)
        {
            List<Result> results = new List<Result>();

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();
                // Check if the election exists
                string checkElection = "SELECT COUNT(*) FROM Elections WHERE ElectionID = @ElectionID";

                SqlCommand checkCmd = new SqlCommand(checkElection, con);
                checkCmd.Parameters.AddWithValue("@ElectionID", electionId);

                int electionExists = (int)checkCmd.ExecuteScalar();

                if (electionExists == 0)
                {
                    return NotFound("Election not found.");
                }

                string query = @"
                SELECT
                    c.FullName,
                    p.PartyName,
                    COUNT(v.VoteID) AS TotalVotes
                FROM Candidates c
                INNER JOIN PoliticalParties p
                    ON c.PartyID = p.PartyID
                LEFT JOIN Votes v
                    ON c.CandidateID = v.CandidateID
                WHERE c.ElectionID = @ElectionID
                GROUP BY c.FullName, p.PartyName
                ORDER BY TotalVotes DESC";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@ElectionID", electionId);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(new Result
                    {
                        CandidateName = reader["FullName"].ToString(),
                        PartyName = reader["PartyName"].ToString(),
                        TotalVotes = Convert.ToInt32(reader["TotalVotes"])
                    });
                }
            }

            return Ok(results);
        }
    }
}