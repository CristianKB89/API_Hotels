using System;

namespace API_Hotels.Models.Outputs
{
    public class HotelCreateResponseOutput
    {
        public Guid HotelId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public decimal BasePrice { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
