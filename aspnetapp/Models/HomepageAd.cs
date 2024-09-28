namespace aspnetapp.Models {
    // 首页广告表
    [Table("T_HomepageAd")]
    public class HomepageAd {
        [Key]
        public int AdId { get; init; }// 广告编号

        public string? PictureLink { get; set; }// 图片链接

        public string? Jumplink { get; set; }// 跳转链接

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
