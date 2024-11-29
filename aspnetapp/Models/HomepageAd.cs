namespace aspnetapp.Models {
    /// <summary>
    /// 首页广告表
    /// </summary>
    [Table("T_HomepageAd")]
    public class HomepageAd {
        /// <summary>
        /// 广告编号
        /// </summary>
        [Key]
        public int Id { get; init; }

		/// <summary>
		/// 图片链接
		/// </summary>
		public string? PictureLink { get; set; }

		/// <summary>
		/// 跳转链接
		/// </summary>
		public string? Jumplink { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreatedAt { get; set; }

		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime UpdatedAt { get; set; }

		/// <summary>
		/// 用于乐观并发控制
		/// </summary>
		[Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
