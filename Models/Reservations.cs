using System;

namespace API_Hotels.Models
{
    public class Reservations
    {
        public Guid ReservationId { get; set; }
        public Guid RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int TotalGuests { get; set; }
        public decimal TotalCost { get; set; }
        public bool EmailNotification { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
