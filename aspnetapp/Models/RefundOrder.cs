namespace aspnetapp.Models {
	/// <summary>
	/// 退款订单表
	/// </summary>
	[Table("T_RefundOrderForm")]
	[Index(nameof(TheOrder))]
	public class RefundOrder {
		/// <summary>
		/// 编号
		/// </summary>
		[Key]
		public int Id { get; init; }
		/// <summary>
		/// 【商户退款单号】商户系统内部的退款单号，商户系统内部唯一，只能是数字、大小写字母_-|*@ ，同一退款单号多次请求只退一笔。原支付交易对应的商户订单号
		/// </summary>
		public string outRefundNo { get; set; } = string.Concat("Refund_", Guid.NewGuid().ToString("N").AsSpan(0, 20));
		/// <summary>
		/// 指向订单
		/// </summary>
		public int TheOrder { get; set; }

		/// <summary>
		/// 【微信支付退款号】微信支付退款号
		/// </summary>
		public string RefundId { get; set; }

		/// <summary>
		/// 【退款原因】若商户传入，会在下发给用户的退款消息中体现退款原因
		/// </summary>
		public string Reason { get; set; }

		/// <summary>
		/// 退款渠道
		/// </summary>
		//public string Channel { get; set; } = "ORIGINAL";// 原路退款

		/// <summary>
		/// 退款状态
		/// </summary>
		public Estatus Status { get; set; }

        /// <summary>
        /// 【原订单金额】原支付交易的订单总金额，单位为分，只能为整数。
        /// </summary>
        [Precision(10, 2)]
        public decimal Total { get; set; }

        /// <summary>
        /// 【退款金额】退款标价金额，单位为分，可以做部分退款
        /// </summary>
        [Precision(10, 2)]
        public decimal Refund { get; set; }

		/// <summary>
		/// 【退款币种】符合ISO 4217标准的三位字母代码，目前只支持人民币：CNY。
		/// </summary>
		//public string Currency { get; set; } = "CNY";

		/// <summary>
		/// 【退款成功时间】退款成功时间，退款状态status为SUCCESS（退款成功）时，返回该字段。
		/// 遵循rfc3339标准格式，格式为YYYY-MM-DDTHH:mm:ss+TIMEZONE
		/// </summary>
		public DateTime? SuccessTime { get; set; }

		/// <summary>
		/// 【退款创建时间】退款受理时间
		/// </summary>
		public DateTime CreateTime { get; set; }

		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime UpdatedAt { get; set; }

		/// <summary>
		/// 【退款状态】
		/// 退款到银行发现用户的卡作废或者冻结了，导致原路退款银行卡失败，可前往商户平台（pay.weixin.qq.com）-交易中心，手动处理此笔退款。
		/// </summary>
		public enum Estatus {
			退款成功,
			退款关闭,
			退款处理中,
			退款异常,
			已创建
		}
	}
}
