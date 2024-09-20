namespace aspnetapp.Models {
    public class User {
        public int Id { get; set; }
        public string? Name { get; set; } = null;
        public string phone { get; set; } = string.Empty;
        public string? Password { get; set; }
        public DateTime createdAt { get; set; } = DateTime.Now;
        public DateTime updatedAt { get; set; }
    }
}
