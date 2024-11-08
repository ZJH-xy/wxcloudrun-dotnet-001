namespace aspnetapp.Models {
    /// <summary>
    /// 门店套餐表
    /// </summary>
    [Table("T_StoreMenu")]
    [Index(nameof(TheStore))]
    public class StoreMenu {
        [Key]
        public int Id { get; init; }

        /* 逻辑指向门店 */
        public int TheStore { get; set; }

        public int Duration { get; set; }// 小时时长

        [Precision(10, 2)]
        public decimal Rent { get; set; }// 租金

        [Precision(10, 2)]
        public decimal Deposit { get; set; }// 押金

        public bool IsDelete { get; set; } = false;

        [Timestamp]
        public byte[] RowVersion { get; set; }// 用于乐观并发控制
    }
}
