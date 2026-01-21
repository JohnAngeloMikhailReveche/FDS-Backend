namespace NotificationService.DTOs
{
    public class RequestOrderNotificationDTO
    {
        public long ChatId { get; set; }
        public string OrderId { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }      
}