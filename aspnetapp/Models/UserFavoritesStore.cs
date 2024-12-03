namespace aspnetapp.Models {
    /// <summary>
    /// 用户收藏表
    /// </summary>
    [Table("T_UserFavoritesStore")]
    [Index(nameof(TheUser))]
    public class UserFavoritesStore {
		/// <summary>
		/// 用户收藏Id
		/// </summary>
		[Key]
        public int Id { get; set; }

		/// <summary>
		/// 逻辑指向用户
		/// </summary>
		public int TheUser { get; set; }

		/// <summary>
		/// 逻辑指向门店
		/// </summary>
		public int TheStore { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreatedAt { get; set; }

		/// <summary>
		/// 用于乐观并发控制
		/// </summary>
		[Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
