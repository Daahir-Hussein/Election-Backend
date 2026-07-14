namespace ElectionManagement.API.Models
{
    public class Voter
    {
        public int VoterID { get; set; }

        public string NationalID { get; set; }

        public string FullName { get; set; }

        public string Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }
    }
}