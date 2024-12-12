namespace aspnetapp.Models {
    /// <summary>
    /// 门店总表
    /// </summary>
    [Table("T_StoreSummary")]
    [Index(nameof(IsDelete))]
    public class Store {
        /// <summary>
        /// 门店编号
        /// </summary>
        [Key]
        public int Id { get; init; }// 

		/// <summary>
		/// 门店名称
		/// </summary>
		[Required(ErrorMessage = "门店名称不可为空")]
        public string Name { get; set; } = string.Empty;

		/// <summary>
		/// 营业开始时间
		/// </summary>
		[DataType(DataType.Time)]//HH:MM:SS
        [Required(ErrorMessage = "营业开始时间不可为空")]
        public TimeSpan BusinessHoursStart { get; set; }

		/// <summary>
		/// 营业结束时间
		/// </summary>
		[DataType(DataType.Time)]//HH:MM:SS
        [Required(ErrorMessage = "营业结束时间不可为空")]
        public TimeSpan BusinessHoursEnd { get; set; }

		/// <summary>
		/// 营业状态
		/// </summary>
		public bool BusinessStatus { get; set; } = true;

		/// <summary>
		/// 联系电话
		/// </summary>
		public string? Telephone { get; set; }

		/// <summary>
		/// 微信
		/// </summary>
		public string? WeChat { get; set;}

		/// <summary>
		/// 门店地址
		/// </summary>
		public string? Address { get; set; }

		/// <summary>
		/// Longitude 经度，范围 [-180, 180]
		/// </summary>
		public double GpsLongitude { get; set; }

		/// <summary>
		/// Latitude 纬度，范围 [-90, 90]
		/// </summary>
		public double GpsLatitude { get; set; }

		/// <summary>
		/// 门店图片
		/// </summary>
		public string? Pictures { get; set; }

		/// <summary>
		/// 介绍
		/// </summary>
		public string? Introduce { get; set; }

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
		/// 判断门店是否在营业时间内
		/// </summary>
		/// <returns>真=>开门，反之关门</returns>
		public bool IsOpen() {
            var now = DateTime.Now.TimeOfDay; // 获取当前时间的时间部分
            return now >= BusinessHoursStart && now <= BusinessHoursEnd; // 判断是否在营业时间内
        }
    }
}
