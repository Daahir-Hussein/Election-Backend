using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ElectionManagement.API.Data;
using ElectionManagement.API.Models;

namespace ElectionManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElectionsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllElections()
        {
            List<Election> elections = new List<Election>();

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = "SELECT * FROM Elections";

                SqlCommand cmd = new SqlCommand(query, con);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    elections.Add(new Election
                    {
                        ElectionID = Convert.ToInt32(reader["ElectionID"]),
                        ElectionName = reader["ElectionName"].ToString(),
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        EndDate = Convert.ToDateTime(reader["EndDate"]),
                        Status = reader["Status"].ToString()
                    });
                }
            }

            return Ok(elections);
        }

        //Get by ID

        [HttpGet("{id}")]
        public IActionResult GetElectionById(int id)
        {
            Election election = null;

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = "SELECT * FROM Elections WHERE ElectionID=@ElectionID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ElectionID", id);

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    election = new Election
                    {
                        ElectionID = Convert.ToInt32(reader["ElectionID"]),
                        ElectionName = reader["ElectionName"].ToString(),
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        EndDate = Convert.ToDateTime(reader["EndDate"]),
                        Status = reader["Status"].ToString()
                    };
                }
            }

            if (election == null)
            {
                return NotFound("Election Not Found");
            }

            return Ok(election);
        }

        // POST

        [HttpPost]
        public IActionResult AddElection(Election election)
        {
            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = @"INSERT INTO Elections
                        (ElectionName, StartDate, EndDate, Status)
                        VALUES
                        (@ElectionName, @StartDate, @EndDate, @Status)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@ElectionName", election.ElectionName);
                cmd.Parameters.AddWithValue("@StartDate", election.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", election.EndDate);
                cmd.Parameters.AddWithValue("@Status", election.Status);

                cmd.ExecuteNonQuery();
            }

            return Ok("Election Added Successfully");
        }

        // UPDATE

        [HttpPut("{id}")]
        public IActionResult UpdateElection(int id, Election election)
        {
            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                string query = @"UPDATE Elections
                         SET ElectionName=@ElectionName,
                             StartDate=@StartDate,
                             EndDate=@EndDate,
                             Status=@Status
                         WHERE ElectionID=@ElectionID";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@ElectionID", id);
                cmd.Parameters.AddWithValue("@ElectionName", election.ElectionName);
                cmd.Parameters.AddWithValue("@StartDate", election.StartDate);
                cmd.Parameters.AddWithValue("@EndDate", election.EndDate);
                cmd.Parameters.AddWithValue("@Status", election.Status);

                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                    return NotFound("Election Not Found");
            }

            return Ok("Election Updated Successfully");
        }

        // DELETE

        [HttpDelete("{id}")]
        public IActionResult DeleteElection(int id)
        {
            try
            {
                using (SqlConnection con = DbConnection.GetConnection())
                {
                    con.Open();

                    string query = "DELETE FROM Elections WHERE ElectionID = @ElectionID";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@ElectionID", id);

                    int rows = cmd.ExecuteNonQuery();

                    if (rows == 0)
                    {
                        return NotFound(new
                        {
                            message = "Election not found."
                        });
                    }
                }

                return Ok(new
                {
                    message = "Election deleted successfully."
                });
            }
            catch (SqlException ex)
            {
                // SQL Server Foreign Key Constraint Error
                if (ex.Number == 547)
                {
                    return BadRequest(new
                    {
                        message = "Cannot delete this election because candidates or vote records are linked to it."
                    });
                }

                return StatusCode(500, new
                {
                    message = "An error occurred while deleting the election."
                });
            }
        }
    }
}