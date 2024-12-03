namespace aspnetapp.Models {
    /// <summary>
    /// 营业额统计表
    /// </summary>
    [Table("T_RevenueStatistics")]
    [Index(nameof(TheStoreA), nameof(TheStoreB))]
    public class RevenueStatistics {
        /// <summary>
        /// 
        /// </summary>
        [Key]
        public int Id { get; init; }

		/// <summary>
		/// 租车点（逻辑指向门店）
		/// </summary>
		public int TheStoreA { get; set; }

		/// <summary>
		/// 还车点（逻辑指向门店）
		/// </summary>
		public int TheStoreB { get; set; }

		/// <summary>
		/// 逻辑指向订单
		/// </summary>
		public int TheOrder { get; set; }

		/// <summary>
		/// 门店A 金额
		/// </summary>
		[Precision(10, 2)]
        public decimal? MoneyStoreA { get; set; }

		/// <summary>
		/// 门店B 金额
		/// </summary>
		[Precision(10, 2)]
        public decimal? MoneyStoreB { get; set; }

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
