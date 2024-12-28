namespace API_Hotels.Models.Inputs
{
    public class AddEmergencyContactRequestInput
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Relationship { get; set; }
    }
}
