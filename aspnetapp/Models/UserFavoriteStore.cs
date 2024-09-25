namespace aspnetapp.Models {
    // 用户收藏门店
    [Table("T_UserFavoriteStore")]
    public class UserFavoriteStore {
        [ForeignKey("Users")]
        public int UserId { get; set; }// 用户编号（UserId）

        [ForeignKey("StoreVehicle")]
        public int StoreId { get; set; }// 门店编号（StoreId）

        public DateTime FavoriteTime { get; set; }// 收藏时间
    }
}
