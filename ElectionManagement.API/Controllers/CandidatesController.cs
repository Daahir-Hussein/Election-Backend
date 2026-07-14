using ElectionManagement.API.Data;
using ElectionManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ElectionManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatesController : ControllerBase
    {
        // Get All

        [HttpGet]
        public IActionResult GetAllCandidates()
        {
            List<Candidate> candidates = new List<Candidate>();

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = "SELECT * FROM Candidates";

                SqlCommand cmd = new SqlCommand(query, con);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    candidates.Add(new Candidate
                    {
                        CandidateID = Convert.ToInt32(reader["CandidateID"]),
                        ElectionID = Convert.ToInt32(reader["ElectionID"]),
                        PartyID = Convert.ToInt32(reader["PartyID"]),
                        FullName = reader["FullName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        Photo = reader["Photo"].ToString(),
                        Biography = reader["Biography"].ToString()
                    });
                }
            }

            return Ok(candidates);
        }

        // Get by ID

        [HttpGet("{id}")]
        public IActionResult GetCandidateById(int id)
        {
            Candidate candidate = null;

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = "SELECT * FROM Candidates WHERE CandidateID=@CandidateID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CandidateID", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    candidate = new Candidate
                    {
                        CandidateID = Convert.ToInt32(reader["CandidateID"]),
                        ElectionID = Convert.ToInt32(reader["ElectionID"]),
                        PartyID = Convert.ToInt32(reader["PartyID"]),
                        FullName = reader["FullName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        Photo = reader["Photo"].ToString(),
                        Biography = reader["Biography"].ToString()
                    };
                }
            }

            if (candidate == null)
                return NotFound("Candidate Not Found");

            return Ok(candidate);
        }

        // POST

        [HttpPost]
        public IActionResult AddCandidate(Candidate candidate)
        {
            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = @"INSERT INTO Candidates
                        (ElectionID, PartyID, FullName, Gender, Photo, Biography)
                        VALUES
                        (@ElectionID, @PartyID, @FullName, @Gender, @Photo, @Biography)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@ElectionID", candidate.ElectionID);
                cmd.Parameters.AddWithValue("@PartyID", candidate.PartyID);
                cmd.Parameters.AddWithValue("@FullName", candidate.FullName);
                cmd.Parameters.AddWithValue("@Gender", candidate.Gender);
                cmd.Parameters.AddWithValue("@Photo", (object?)candidate.Photo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Biography", (object?)candidate.Biography ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            return Ok("Candidate Added Successfully");
        }

        // UPDATE

        [HttpPut("{id}")]
        public IActionResult UpdateCandidate(int id, Candidate candidate)
        {
            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = @"UPDATE Candidates
                        SET ElectionID=@ElectionID,
                            PartyID=@PartyID,
                            FullName=@FullName,
                            Gender=@Gender,
                            Photo=@Photo,
                            Biography=@Biography
                        WHERE CandidateID=@CandidateID";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@CandidateID", id);
                cmd.Parameters.AddWithValue("@ElectionID", candidate.ElectionID);
                cmd.Parameters.AddWithValue("@PartyID", candidate.PartyID);
                cmd.Parameters.AddWithValue("@FullName", candidate.FullName);
                cmd.Parameters.AddWithValue("@Gender", candidate.Gender);
                cmd.Parameters.AddWithValue("@Photo", (object?)candidate.Photo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Biography", (object?)candidate.Biography ?? DBNull.Value);

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                    return NotFound("Candidate Not Found");
            }

            return Ok("Candidate Updated Successfully");
        }

        // DELETE

        

[HttpDelete("{id}")]
    public IActionResult DeleteCandidate(int id)
    {
        try
        {
            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = "DELETE FROM Candidates WHERE CandidateID = @CandidateID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CandidateID", id);

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                {
                    return NotFound(new
                    {
                        message = "Candidate not found."
                    });
                }
            }

            return Ok(new
            {
                message = "Candidate deleted successfully."
            });
        }
        catch (SqlException ex)
        {
            // Foreign Key Constraint Error
            if (ex.Number == 547)
            {
                return BadRequest(new
                {
                    message = "Cannot delete this candidate because vote records exist for this candidate."
                });
            }

            return StatusCode(500, new
            {
                message = "An error occurred while deleting the candidate."
            });
        }
    }

    [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected.");

            var fileName = Guid.NewGuid().ToString()
                           + Path.GetExtension(file.FileName);

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "candidates");

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { fileName });
        }
    }
}