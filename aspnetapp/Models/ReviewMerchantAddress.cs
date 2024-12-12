namespace aspnetapp.Models {
    /// <summary>
    /// 审核商家地址表
    /// </summary>
    [Table("T_ReviewMerchantAddress")]
    public class ReviewMerchantAddress {
        /// <summary>
        /// 审核商家地址表ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 商家
        /// </summary>
        public int TheStore { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string? Address { get; set; }

		/// <summary>
		/// 地址名
		/// </summary>
		public string? Name { get; set; }

        /// <summary>
        /// Longitude 经度，范围 [-180, 180]
        /// </summary>
        public double GpsLongitude { get; init; }

        /// <summary>
        /// Latitude 纬度，范围 [-90, 90]
        /// </summary>
        public double GpsLatitude { get; init; }

        /// <summary>
        /// 用于乐观并发控制
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
