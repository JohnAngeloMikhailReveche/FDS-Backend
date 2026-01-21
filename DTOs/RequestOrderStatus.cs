namespace NotificationService.DTOs
{
    public class RequestOrderStatusDTO
    {
        public long ChatId { get; set; }
        public string OrderId { get; set; } = null!;
    }
}