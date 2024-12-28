namespace API_Hotels.Models.Inputs
{
    public class HotelUpdateRequestInput
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public decimal BasePrice { get; set; }
        public bool Status { get; set; }
    }
}
