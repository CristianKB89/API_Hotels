namespace API_Hotels.Models.Inputs
{
    public class HotelCreateRequestInput
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public decimal BasePrice { get; set; }
    }
}
