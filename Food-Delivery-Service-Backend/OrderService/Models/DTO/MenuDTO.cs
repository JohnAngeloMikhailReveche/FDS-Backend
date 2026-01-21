namespace OrderService.Models.DTO
{
    public class MenuDTO
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string imgUrl { get; set; }
        public bool isAvailable { get; set; }
        public List<VariantsDTO> variants { get; set; }
    }
}
