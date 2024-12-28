using System;

namespace API_Hotels.Models
{
    public class Rooms
    {
        public Guid RoomId { get; set; }
        public Guid HotelId { get; set; }
        public string RoomType { get; set; }
        public decimal BaseCost { get; set; }
        public decimal TaxPercentage { get; set; }
        public string Location { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
