namespace aspnetapp.Models {
    /// <summary>
    /// 营业额统计表
    /// </summary>
    [Table("T_RevenueStatistics")]
    [Index(nameof(TheStoreA), nameof(TheStoreB))]
    public class RevenueStatistics {
        [Key]
        public int Id { get; set; }

        /* 逻辑指向门店 */
        public int TheStoreA { get; set; }// 租车点

        /* 逻辑指向门店 */
        public int TheStoreB { get; set; }// 还车点

        /* 逻辑指向订单 */
        public int TheOrder { get; set; }

        [Precision(10, 2)]
        public decimal? MoneyStoreA { get; set; }// 门店A 金额

        [Precision(10, 2)]
        public decimal? MoneyStoreB { get; set; }// 门店B 金额

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
