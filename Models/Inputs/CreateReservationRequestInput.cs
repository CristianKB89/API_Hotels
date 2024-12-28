using System;

namespace API_Hotels.Models.Inputs
{
    public class CreateReservationRequestInput
    {
        public Guid HotelId { get; set; }
        public Guid RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string GuestName { get; set; }
        public int NumberOfGuests { get; set; }
    }
}
