namespace aspnetapp.Models {
    /// <summary>
    /// 换车记录表
    /// </summary>
    [Table("T_VehicleReplacementRecord")]
    [Index(nameof(TheOrder))]
    public class VehicleReplacementRecord {
		/// <summary>
		/// 换车记录Id
		/// </summary>
		[Key]
        public int Id { get; set; }

        /// <summary>
        /// 逻辑指向订单
        /// </summary>
        public int TheOrder { get; set; }

		/// <summary>
		/// 旧车辆（逻辑指向车辆）
		/// </summary>
		public int TheOldVehicles { get; set; }

		/// <summary>
		/// 新车辆（逻辑指向车辆）
		/// </summary>
		public int TheNewVehicles { get; set; }

		/// <summary>
		/// 换车门店（逻辑指向门店）
		/// </summary>
		public int TheStore { get; set; }

		/// <summary>
		/// 换车记录状态
		/// </summary>
		public Estates State { get; set; }

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

		/// <summary>
		/// 换车门店名称
		/// </summary>
		[NotMapped]
		public string StoreName { get; set; }

		/// <summary>
		/// 旧车辆车牌号
		/// </summary>
		[NotMapped]
		public string OldVehiclePlateNumber { get; set; }

		/// <summary>
		/// 新车辆车牌号
		/// </summary>
		[NotMapped]
		public string NewVehiclePlateNumber { get; set; }

		/// <summary>
		/// 换车记录状态枚举
		/// </summary>
		public enum Estates {
            已取消,
            侍确认,
            已完成
        }
    }
}
