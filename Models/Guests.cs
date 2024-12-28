using System;

namespace API_Hotels.Models
{
    public class Guests
    {
        public Guid GuestId { get; set; }
        public Guid ReservationId { get; set; }
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
