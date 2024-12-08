namespace aspnetapp.Models {
	/// <summary>
	/// 订单表
	/// </summary>
	[Table("T_OrderForm")]
	[Index(nameof(TransactionId), IsUnique = true)]// 唯一索引
	[Index(nameof(TheUser), nameof(Status))]
	public class Order {
		/// <summary>
		/// 订单编号
		/// </summary>
		[Key]
		public int Id { get; init; }

        /// <summary>
        /// 商户系统内部订单号，可以是数字、大小写字母_-*的任意组合且在同一个商户号下唯一。
        /// </summary>
        public string? OutTradeNo { get; set; }

        /// <summary>
        /// 微信支付系统生成的订单号
        /// </summary>
        public string? TransactionId { get; set; }

		/// <summary>
		/// 逻辑指向用户
		/// </summary>
		public int TheUser { get; set; }

		/// <summary>
		/// 逻辑指向套餐
		/// </summary>
		public int TheStoreMenu { get; set; }

		/// <summary>
		/// 实际起始时间
		/// </summary>
		public DateTime? ActualStartingTime { get; set; }

		/// <summary>
		/// 实际归还时间
		/// </summary>
		public DateTime? ActualReturnTime { get; set; }

		/// <summary>
		/// 逻辑指向车辆（租用车辆）
		/// </summary>
		public int TheVehicle { get; set; }

		/// <summary>
		/// 租车点（StoreId）（逻辑指向门店）
		/// </summary>
		public int TheRentalLocation { get; set; }

		/// <summary>
		/// 还车点（StoreId）（逻辑指向门店）
		/// </summary>
		public int? TheReturnThePoint { get; set; }

		/// <summary>
		/// 用户姓名
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// 用户手机号
		/// </summary>
		public string UserPhone { get; set; }

		/// <summary>
		/// 身份证号
		/// </summary>
		public string? IdentityCard { get; set; }

		/// <summary>
		/// 押金
		/// </summary>
		[Precision(10, 2)]
		public decimal Deposit { get; set; }

		/// <summary>
		/// 租金
		/// </summary>
		[Precision(10, 2)]
		public decimal Rent { get; set; }

		/// <summary>
		/// 调度费
		/// </summary>
		[Precision(10, 2)]
		public decimal DispatchFee { get; set; } = 0;

		/// <summary>
		/// 超时费
		/// </summary>
		[Precision(10, 2)]
		public decimal OvertimeFee { get; set; } = 0;

		/// <summary>
		/// 其他费用
		/// </summary>
		[Precision(10, 2)]
		public decimal OtherFees { get; set; } = 0;

		/// <summary>
		/// 已付
		/// </summary>
		[Precision(10, 2)]
		public decimal Paid { get; set; } = 0;

		/// <summary>
		/// 已退押金
		/// </summary>
		[Precision(10, 2)]
		public decimal DepositRefunded { get; set; } = 0;

		/// <summary>
		/// 订单状态
		/// </summary>
		public EOrderStatus Status { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		public string? Notes { get; set; }

		/// <summary>
		/// 支付完成时间（微信获取）
		/// </summary>
		public DateTime? SuccessTime { get; set; }

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
		/// 订单状态枚举
		/// </summary>
		public enum EOrderStatus {
			已取消,
			待付款,
			已退款,
			待确认,
			进行中,
			已完成,
			付款中,
			退款中,
			侍补余,
            已补余
        }

		/// <summary>
		/// 获取订单总金额
		/// </summary>
		/// <returns></returns>
		public decimal GetTotalPrice() {
			return Deposit + Rent + DispatchFee + OvertimeFee + OtherFees;
		}

		/// <summary>
		/// 转换为以分为单位的金额
		/// </summary>
		/// <param name="rentInYuan">单位元</param>
		/// <returns></returns>
		public static int GetTotal(decimal rentInYuan) {
			return (int)Math.Round(rentInYuan * 100, MidpointRounding.AwayFromZero);
		}
	}
}
