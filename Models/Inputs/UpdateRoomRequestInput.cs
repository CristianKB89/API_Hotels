namespace API_Hotels.Models.Inputs
{
    public class UpdateRoomRequestInput
    {
        public string RoomType { get; set; }
        public decimal BaseCost { get; set; }
        public decimal TaxPercentage { get; set; }
        public bool Status { get; set; }
    }
}
