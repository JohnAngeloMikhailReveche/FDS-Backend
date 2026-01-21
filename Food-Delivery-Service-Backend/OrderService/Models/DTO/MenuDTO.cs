namespace OrderService.Models.DTO
{
    public class MenuDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string imageUrl { get; set; }
        public bool isAvailable { get; set; }
        public List<VariantsDTO> variants { get; set; }
    }
}
