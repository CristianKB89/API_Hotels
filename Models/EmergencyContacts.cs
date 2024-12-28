using System;

namespace API_Hotels.Models
{
    public class EmergencyContacts
    {
        public Guid ContactId { get; set; }
        public Guid GuestId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
    }
}
