using ElectionManagement.API.Data;
using ElectionManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ElectionManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PoliticalPartiesController : ControllerBase
    {
        // Get All
        [HttpGet]
        public IActionResult GetAllParties()
        {
            List<PoliticalParty> parties = new List<PoliticalParty>();

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = "SELECT * FROM PoliticalParties";

                SqlCommand cmd = new SqlCommand(query, con);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    parties.Add(new PoliticalParty
                    {
                        PartyID = Convert.ToInt32(reader["PartyID"]),
                        PartyName = reader["PartyName"].ToString(),
                        LeaderName = reader["LeaderName"].ToString(),
                        Logo = reader["Logo"].ToString()
                    });
                }
            }

            return Ok(parties);
        }

        // Get by ID
        [HttpGet("{id}")]
        public IActionResult GetPartyById(int id)
        {
            PoliticalParty party = null;

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = "SELECT * FROM PoliticalParties WHERE PartyID=@PartyID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PartyID", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    party = new PoliticalParty
                    {
                        PartyID = Convert.ToInt32(reader["PartyID"]),
                        PartyName = reader["PartyName"].ToString(),
                        LeaderName = reader["LeaderName"].ToString(),
                        Logo = reader["Logo"].ToString()
                    };
                }
            }

            if (party == null)
                return NotFound("Political Party Not Found");

            return Ok(party);
        }

        // POST

        [HttpPost]
        public IActionResult AddParty(PoliticalParty party)
        {
            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = @"INSERT INTO PoliticalParties
                        (PartyName, LeaderName, Logo)
                        VALUES
                        (@PartyName, @LeaderName, @Logo)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@PartyName", party.PartyName);
                cmd.Parameters.AddWithValue("@LeaderName", party.LeaderName);
                cmd.Parameters.AddWithValue("@Logo", party.Logo ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            return Ok("Political Party Added Successfully");
        }

        // UPDATE

        [HttpPut("{id}")]
        public IActionResult UpdateParty(int id, PoliticalParty party)
        {
            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = @"UPDATE PoliticalParties
                        SET PartyName=@PartyName,
                            LeaderName=@LeaderName,
                            Logo=@Logo
                        WHERE PartyID=@PartyID";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@PartyID", id);
                cmd.Parameters.AddWithValue("@PartyName", party.PartyName);
                cmd.Parameters.AddWithValue("@LeaderName", party.LeaderName);
                cmd.Parameters.AddWithValue("@Logo", party.Logo ?? (object)DBNull.Value);

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                    return NotFound("Political Party Not Found");
            }

            return Ok("Political Party Updated Successfully");
        }

        // DELETE

        

        [HttpDelete("{id}")]
            public IActionResult DeleteParty(int id)
            {
                try
                {
                    using (SqlConnection con = DbConnection.GetConnection())
                    {
                        con.Open();

                        string query = "DELETE FROM PoliticalParties WHERE PartyID = @PartyID";

                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@PartyID", id);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows == 0)
                        {
                            return NotFound(new
                            {
                                message = "Political party not found."
                            });
                        }
                    }

                    return Ok(new
                    {
                        message = "Political party deleted successfully."
                    });
                }
                catch (SqlException ex)
                {
                    // Foreign Key Constraint Error
                    if (ex.Number == 547)
                    {
                        return BadRequest(new
                        {
                            message = "Cannot delete this political party because candidates are registered under it."
                        });
                    }

                    return StatusCode(500, new
                    {
                        message = "An error occurred while deleting the political party."
                    });
                }
            }

    [HttpPost("upload")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file selected.");

            var fileName = Guid.NewGuid().ToString()
                           + Path.GetExtension(file.FileName);

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "parties");

            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { fileName });
        }
    }
}