using ElectionManagement.API.Data;
using ElectionManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ElectionManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetDashboard()
        {
            Dashboard dashboard = new Dashboard();

            using (SqlConnection con = DbConnection.GetConnection())
            {
                con.Open();

                SqlCommand cmd;

                cmd = new SqlCommand("SELECT COUNT(*) FROM Elections", con);
                dashboard.TotalElections = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM PoliticalParties", con);
                dashboard.TotalParties = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Candidates", con);
                dashboard.TotalCandidates = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Voters", con);
                dashboard.TotalVoters = (int)cmd.ExecuteScalar();

                cmd = new SqlCommand("SELECT COUNT(*) FROM Votes", con);
                dashboard.TotalVotes = (int)cmd.ExecuteScalar();
            }

            return Ok(dashboard);
        }
    }
}