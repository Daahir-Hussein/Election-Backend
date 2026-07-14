using ElectionManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ElectionManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoterPortalController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public VoterPortalController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("Login")]
        public IActionResult Login(VoterLoginRequest request)
        {
            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using SqlConnection con = new SqlConnection(connectionString);

            con.Open();

            string query = @"
                SELECT
                    VoterID,
                    NationalID,
                    FullName
                FROM Voters
                WHERE NationalID = @NationalID";

            using SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue(
                "@NationalID",
                request.NationalID
            );

            using SqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                return NotFound("Voter not found.");
            }

            return Ok(new
            {
                VoterID = Convert.ToInt32(reader["VoterID"]),
                NationalID = reader["NationalID"].ToString(),
                FullName = reader["FullName"].ToString()
            });
        }

        [HttpGet("Elections")]
        public IActionResult GetOpenElections()
        {
            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using SqlConnection con = new SqlConnection(connectionString);

            con.Open();

            string query = @"
        SELECT
            ElectionID,
            ElectionName,
            StartDate,
            EndDate
        FROM Elections
        WHERE Status = 'Open'
        ORDER BY StartDate";

            using SqlCommand cmd = new SqlCommand(query, con);

            using SqlDataReader reader = cmd.ExecuteReader();

            List<object> elections = new();

            while (reader.Read())
            {
                elections.Add(new
                {
                    ElectionID = Convert.ToInt32(reader["ElectionID"]),
                    ElectionName = reader["ElectionName"].ToString(),
                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                    EndDate = Convert.ToDateTime(reader["EndDate"])
                });
            }

            return Ok(elections);
        }

        [HttpGet("Candidates/{electionId}")]
        public IActionResult GetCandidatesByElection(int electionId)
        {
            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using SqlConnection con = new SqlConnection(connectionString);

            con.Open();

            string electionCheckQuery =
                "SELECT COUNT(*) FROM Elections WHERE ElectionID = @ElectionID";

            using SqlCommand electionCmd =
                new SqlCommand(electionCheckQuery, con);

            electionCmd.Parameters.AddWithValue("@ElectionID", electionId);

            int electionExists =
                Convert.ToInt32(electionCmd.ExecuteScalar());

            if (electionExists == 0)
            {
                return NotFound("Election not found.");
            }

            string query = @"
            SELECT
                c.CandidateID,
                c.FullName AS CandidateName,
                c.Biography,
                c.Photo,
                p.PartyName,
                p.Logo AS PartyLogo
            FROM Candidates c
            INNER JOIN PoliticalParties p
                ON c.PartyID = p.PartyID
            WHERE c.ElectionID = @ElectionID";

            using SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddWithValue("@ElectionID", electionId);

            using SqlDataReader reader = cmd.ExecuteReader();

            List<object> candidates = new();

            while (reader.Read())
            {
                candidates.Add(new
                {
                    CandidateID = Convert.ToInt32(reader["CandidateID"]),
                    CandidateName = reader["CandidateName"].ToString(),
                    Biography = reader["Biography"]?.ToString(),
                    Photo = reader["Photo"]?.ToString(),
                    PartyName = reader["PartyName"]?.ToString(),
                    PartyLogo = reader["PartyLogo"]?.ToString()
                });
            }

            return Ok(candidates);
        }

        [HttpPost("Vote")]
        public IActionResult Vote(VoteRequest request)
        {
            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            using SqlConnection con = new SqlConnection(connectionString);

            con.Open();

            // 1. Election Exists

            string electionQuery =
                "SELECT COUNT(*) FROM Elections WHERE ElectionID=@ElectionID";

            using SqlCommand electionCmd =
                new SqlCommand(electionQuery, con);

            electionCmd.Parameters.AddWithValue(
                "@ElectionID",
                request.ElectionID
            );

            if (Convert.ToInt32(electionCmd.ExecuteScalar()) == 0)
            {
                return NotFound("Election not found.");
            }

            // 2. Candidate belongs to election

            string candidateQuery = @"
        SELECT COUNT(*)
        FROM Candidates
        WHERE CandidateID=@CandidateID
        AND ElectionID=@ElectionID";

            using SqlCommand candidateCmd =
                new SqlCommand(candidateQuery, con);

            candidateCmd.Parameters.AddWithValue(
                "@CandidateID",
                request.CandidateID
            );

            candidateCmd.Parameters.AddWithValue(
                "@ElectionID",
                request.ElectionID
            );

            if (Convert.ToInt32(candidateCmd.ExecuteScalar()) == 0)
            {
                return BadRequest(
                    "Candidate does not belong to the selected election."
                );
            }

            // 3. Voter Exists

            string voterQuery =
                "SELECT COUNT(*) FROM Voters WHERE VoterID=@VoterID";

            using SqlCommand voterCmd =
                new SqlCommand(voterQuery, con);

            voterCmd.Parameters.AddWithValue(
                "@VoterID",
                request.VoterID
            );

            if (Convert.ToInt32(voterCmd.ExecuteScalar()) == 0)
            {
                return NotFound("Voter not found.");
            }

            // 4. Already voted?

            string votedQuery = @"
        SELECT COUNT(*)
        FROM Votes
        WHERE VoterID=@VoterID
        AND ElectionID=@ElectionID";

            using SqlCommand votedCmd =
                new SqlCommand(votedQuery, con);

            votedCmd.Parameters.AddWithValue(
                "@VoterID",
                request.VoterID
            );

            votedCmd.Parameters.AddWithValue(
                "@ElectionID",
                request.ElectionID
            );

            if (Convert.ToInt32(votedCmd.ExecuteScalar()) > 0)
            {
                return BadRequest(
                    "You have already voted in this election."
                );
            }

            // 5. Insert vote

            string insertQuery = @"
        INSERT INTO Votes
        (
            VoterID,
            CandidateID,
            ElectionID,
            VoteDate
        )
        VALUES
        (
            @VoterID,
            @CandidateID,
            @ElectionID,
            GETDATE()
        )";

            using SqlCommand insertCmd =
                new SqlCommand(insertQuery, con);

            insertCmd.Parameters.AddWithValue(
                "@VoterID",
                request.VoterID
            );

            insertCmd.Parameters.AddWithValue(
                "@CandidateID",
                request.CandidateID
            );

            insertCmd.Parameters.AddWithValue(
                "@ElectionID",
                request.ElectionID
            );

            insertCmd.ExecuteNonQuery();

            return Ok("Vote submitted successfully.");
        }
    }
}