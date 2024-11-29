namespace aspnetapp.Models {
    /// <summary>
    /// 门店套餐表
    /// </summary>
    [Table("T_StoreMenu")]
    [Index(nameof(TheStore))]
    public class StoreMenu {
		/// <summary>
		/// 门店套餐Id
		/// </summary>
		[Key]
        public int Id { get; set; }
        /// <summary>
        /// 逻辑指向门店
        /// </summary>
        public int TheStore { get; set; }

		/// <summary>
		/// 小时时长
		/// </summary>
		public int Duration { get; set; }

		/// <summary>
		/// 租金
		/// </summary>
		[Precision(10, 2)]
        public decimal Rent { get; set; }

		/// <summary>
		/// 押金
		/// </summary>
		[Precision(10, 2)]
        public decimal Deposit { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool IsDelete { get; set; } = false;

		/// <summary>
		/// 用于乐观并发控制
		/// </summary>
		[Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
