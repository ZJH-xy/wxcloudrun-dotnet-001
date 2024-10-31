namespace aspnetapp.Models {
    /// <summary>
    /// 门店套餐表
    /// </summary>
    [Table("T_StoreMenu")]
    [Index(nameof(TheStore))]
    public class StoreMenu {
        [Key]
        public int Id { get; set; }

        /* 逻辑指向门店 */
        public int TheStore { get; set; }

        public int Days { get; set; }// 天数时长

        [Precision(10, 2)]
        public decimal Rent { get; set; }// 租金

        [Precision(10, 2)]
        public decimal Deposit { get; set; }// 押金

        public bool IsDelete { get; set; } = false;
    }
}
