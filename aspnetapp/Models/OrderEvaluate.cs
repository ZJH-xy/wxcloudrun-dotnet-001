namespace aspnetapp.Models {
	/// <summary>
	/// 订单评价表
	/// </summary>
	public class OrderEvaluate {
		/// <summary>
		/// 订单评价编号
		/// </summary>
		[Key]
		public int Id { get; init; }

		/// <summary>
		/// 订单编号
		/// </summary>
		public int TheOrder { get; set; }

		/// <summary>
		/// 用户评分（1-10分）
		/// </summary>
		public int Score { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		public string? Notes { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreatedAt { get; set; }

		/// <summary>
		/// 用于乐观并发控制
		/// </summary>
		[Timestamp]
		public byte[] RowVersion { get; set; }
	}
}
