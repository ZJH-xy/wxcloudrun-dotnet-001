namespace aspnetapp.Models {
	/// <summary>
	/// 订单操作日志表
	/// </summary>
	[Table("T_OrderLog")]
	public class OrderLog {
		/// <summary>
		/// 日志ID
		/// </summary>
		[Key]
		public int Id { get; set; }

		/// <summary>
		/// 关联订单ID
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// 操作管理员ID
		/// </summary>
		public int AdminId { get; set; }

		/// <summary>
		/// 操作类型
		/// </summary>
		public OperationType OperationType { get; set; }

		/// <summary>
		/// 操作时间
		/// </summary>
		public DateTime OperationTime { get; set; }

		/// <summary>
		/// 操作前的订单详细信息（JSON格式）
		/// </summary>
		public string BeforeOrderDetails { get; set; }

		/// <summary>
		/// 操作后的订单详细信息（JSON格式）
		/// </summary>
		public string AfterOrderDetails { get; set; }

		/// <summary>
		/// 备注信息
		/// </summary>
		public string? Notes { get; set; }
	}

	/// <summary>
	/// 操作类型枚举
	/// </summary>
	public enum OperationType {
		其他,
		费用更改,
		添加备注
	}
}
