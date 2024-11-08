namespace aspnetapp.Models {
    /// <summary>
    /// 用户收藏表
    /// </summary>
    [Table("T_UserFavoritesStore")]
    [Index(nameof(TheUser))]
    public class UserFavoritesStore {
        [Key]
        public int Id { get; set; }

        /* 逻辑指向用户 */
        public int TheUser { get; set; }

        /* 逻辑指向门店 */
        public int TheStore { get; set; }

        public DateTime CreatedAt { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }// 用于乐观并发控制
    }
}
