namespace OrderService.Models.DTO
{
    public class AdminCancellationReviewDto
    {
        public int OrderId { get; set; }
        public bool Approve { get; set; }
    }
}