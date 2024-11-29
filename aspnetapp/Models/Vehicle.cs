namespace aspnetapp.Models {
    /// <summary>
    /// 车辆总表
    /// </summary>
    [Table("T_VehicleSummary")]
    [Index(nameof(TheOriginalStore), nameof(TheCurrentStore), nameof(State))]
    public class Vehicle {
        /// <summary>
        /// 车辆编号
        /// </summary>
        [Key]
        public int Id { get; init; }

		/// <summary>
		/// 原始门店（逻辑指向门店）
		/// </summary>
		public int? TheOriginalStore { get; set; }

		/// <summary>
		/// 当前门店（逻辑指向门店）
		/// </summary>
		public int? TheCurrentStore { get; set; }

		/// <summary>
		/// 车辆型号
		/// </summary>
		public Emodel Model { get; set; } = Emodel.未知;

		/// <summary>
		/// 车牌牌号
		/// </summary>
		public string? PlateNumber { get; set; }

		/// <summary>
		/// 车架号
		/// </summary>
		public string? FrameNumber { get; set; }

		/// <summary>
		/// 合格证
		/// </summary>
		public bool? Certificate { get; set; }

		/// <summary>
		/// 发票
		/// </summary>
		public bool? Invoice { get; set; }

		/// <summary>
		/// 行驶证
		/// </summary>
		public bool? Drivinglicense { get; set; }

		/// <summary>
		/// 购入登记时间
		/// </summary>
		public DateTime? PurchaseRegistrationTime { get; set; }

		/// <summary>
		/// 车主
		/// </summary>
		public string? Owner { get; set; }

		/// <summary>
		/// 车辆介绍
		/// </summary>
		public string? VehicleIntroduction { get; set; }

		/// <summary>
		/// 车辆图片
		/// </summary>
		public string? Pictures { get; set; }

		/// <summary>
		/// 车辆状态
		/// </summary>
		public Estates State { get; set; }

		/// <summary>
		/// 是否涉案
		/// </summary>
		public bool IsCase { get; set; } = false; 

		/// <summary>
		/// 状态更新时间
		/// </summary>
		public DateTime StateUpdatedAt { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreatedAt { get; set; }

		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime UpdatedAt { get; set; }

		/// <summary>
		/// 是否删除
		/// </summary>
		public bool IsDelete { get; set; } = false;

		/// <summary>
		/// 用于乐观并发控制
		/// </summary>
		[Timestamp]
        public byte[] RowVersion { get; set; }

		/// <summary>
		/// 车辆状态
		/// </summary>
		public enum Estates {
            空闲,
            已出租,
            充电中,
            故障,
            锁定,
            侍确认
        }

        /// <summary>
		/// 车辆型号
		/// </summary>
        public enum Emodel {
            未知,
            雅迪,
            爱玛,
            台铃
        }
    }
}
