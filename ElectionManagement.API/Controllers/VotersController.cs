using ElectionManagement.API.Data;
using ElectionManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ElectionManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VotersController : ControllerBase
    {
        // Get All

        [HttpGet]
        public IActionResult GetAllVoters()
        {
            List<Voter> voters = new List<Voter>();

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = "SELECT * FROM Voters";

                SqlCommand cmd = new SqlCommand(query, con);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    voters.Add(new Voter
                    {
                        VoterID = Convert.ToInt32(reader["VoterID"]),
                        NationalID = reader["NationalID"].ToString(),
                        FullName = reader["FullName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        DateOfBirth = Convert.ToDateTime(reader["DateOfBirth"]),
                        Phone = reader["Phone"].ToString(),
                        Address = reader["Address"]?.ToString()
                    });
                }
            }

            return Ok(voters);
        }

        // Get by ID

        [HttpGet("{id}")]
        public IActionResult GetVoterById(int id)
        {
            Voter voter = null;

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = "SELECT * FROM Voters WHERE VoterID=@VoterID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@VoterID", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    voter = new Voter
                    {
                        VoterID = Convert.ToInt32(reader["VoterID"]),
                        NationalID = reader["NationalID"].ToString(),
                        FullName = reader["FullName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        DateOfBirth = reader["DateOfBirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["DateOfBirth"]),
                        Phone = reader["Phone"]?.ToString(),
                        Address = reader["Address"]?.ToString()
                    };
                }
            }

            if (voter == null)
                return NotFound("Voter Not Found");

            return Ok(voter);
        }

        // POST

        [HttpPost]
        public IActionResult AddVoter(Voter voter)
        {
            if (voter.DateOfBirth.HasValue &&
                voter.DateOfBirth.Value.Date >= DateTime.Today)
            {
                return BadRequest("Date of birth must be a past date.");
            }

            if (voter.DateOfBirth.HasValue)
            {
                int age = DateTime.Today.Year - voter.DateOfBirth.Value.Year;

                if (voter.DateOfBirth.Value.Date > DateTime.Today.AddYears(-age))
                {
                    age--;
                }

                if (age < 18)
                {
                    return BadRequest("Voter must be at least 18 years old.");
                }
            }

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = @"INSERT INTO Voters
                        (NationalID, FullName, Gender, DateOfBirth, Phone, Address)
                        VALUES
                        (@NationalID, @FullName, @Gender, @DateOfBirth, @Phone, @Address)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@NationalID", voter.NationalID);
                cmd.Parameters.AddWithValue("@FullName", voter.FullName);
                cmd.Parameters.AddWithValue("@Gender", voter.Gender);
                cmd.Parameters.AddWithValue("@DateOfBirth", voter.DateOfBirth);
                cmd.Parameters.AddWithValue("@Phone", voter.Phone);
                cmd.Parameters.AddWithValue("@Address",(object?)voter.Address ?? DBNull.Value);

                cmd.ExecuteNonQuery();
            }

            return Ok("Voter Added Successfully");
        }

        // UPDATE

        [HttpPut("{id}")]
        public IActionResult UpdateVoter(int id, Voter voter)
        {

            if (voter.DateOfBirth.HasValue &&
                voter.DateOfBirth.Value.Date >= DateTime.Today)
            {
                return BadRequest("Date of birth must be a past date.");
            }

            if (voter.DateOfBirth.HasValue)
            {
                int age = DateTime.Today.Year - voter.DateOfBirth.Value.Year;

                if (voter.DateOfBirth.Value.Date > DateTime.Today.AddYears(-age))
                {
                    age--;
                }

                if (age < 18)
                {
                    return BadRequest("Voter must be at least 18 years old.");
                }
            }

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = @"UPDATE Voters
                        SET NationalID=@NationalID,
                            FullName=@FullName,
                            Gender=@Gender,
                            DateOfBirth=@DateOfBirth,
                            Phone=@Phone,
                            Address=@Address
                        WHERE VoterID=@VoterID";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@VoterID", id);
                cmd.Parameters.AddWithValue("@NationalID", voter.NationalID);
                cmd.Parameters.AddWithValue("@FullName", voter.FullName);
                cmd.Parameters.AddWithValue("@Gender", voter.Gender);
                cmd.Parameters.AddWithValue("@DateOfBirth", voter.DateOfBirth);
                cmd.Parameters.AddWithValue("@Phone", voter.Phone);
                cmd.Parameters.AddWithValue("@Address",(object?)voter.Address ?? DBNull.Value);


                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                    return NotFound("Voter Not Found");
            }

            return Ok("Voter Updated Successfully");
        }

        // DELETE

        

        [HttpDelete("{id}")]
            public IActionResult DeleteVoter(int id)
            {
                try
                {
                    using (SqlConnection con = DbConnection.GetConnection())
                    {
                        con.Open();

                        string query = "DELETE FROM Voters WHERE VoterID = @VoterID";

                        SqlCommand cmd = new SqlCommand(query, con);
                        cmd.Parameters.AddWithValue("@VoterID", id);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows == 0)
                        {
                            return NotFound(new
                            {
                                message = "Voter not found."
                            });
                        }
                    }

                    return Ok(new
                    {
                        message = "Voter deleted successfully."
                    });
                }
                catch (SqlException ex)
                {
                    // Foreign Key Constraint Error
                    if (ex.Number == 547)
                    {
                        return BadRequest(new
                        {
                            message = "Cannot delete this voter because they have already participated in an election."
                        });
                    }

                    return StatusCode(500, new
                    {
                        message = "An error occurred while deleting the voter."
                    });
                }
            }
}
}