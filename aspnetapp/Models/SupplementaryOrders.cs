using static aspnetapp.Models.Order;

namespace aspnetapp.Models {
    /// <summary>
    /// 补充订单
    /// </summary>
    public class SupplementaryOrders {
        /// <summary>
        /// 补充订单编号
        /// </summary>
        [Key]
        public int Id { get; init; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public int TheOrder {get; set;}
        /// <summary>
        /// 商户系统内部订单号，可以是数字、大小写字母_-*的任意组合且在同一个商户号下唯一。
        /// </summary>
        public string? OutTradeNo { get; set; }

        /// <summary>
        /// 微信支付系统生成的订单号
        /// </summary>
        public string? TransactionId { get; set; }
        /// <summary>
        /// 【总金额】订单总金额
        /// </summary>
        [Precision(10, 2)]
        public decimal Total { get; set; } = 0;
        /// <summary>
        /// 已付
        /// </summary>
        [Precision(10, 2)]
        public decimal Paid { get; set; } = 0;
        /// <summary>
		/// 订单状态
		/// </summary>
		public EOrderStatus Status { get; set; }
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
    }
}
