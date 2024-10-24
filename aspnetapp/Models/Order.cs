namespace aspnetapp.Models {
    // 订单表
    [Table("T_OrderForm")]
    [Index(nameof(TheUser))]
    public class Order {
        [Key]
        public int OrderId { get; init; }// 订单编号

        /* 逻辑指向用户 */
        public int TheUser { get; set; }

        public DateTime StartingTime { get; set; }// 起始时间

        public DateTime ExpectedReturnTime { get; set; }// 预计归还时间

        public DateTime? ActualReturnTime { get; set; }// 实际归还时间

        /* 逻辑指向车辆 */
        public int Vehicle { get; set; }// 租用车辆

        /* 逻辑指向门店 */
        public int RentalLocation { get; set; }// 租车点（StoreId）

        /* 逻辑指向门店 */
        public int? TransferPoint { get; set; }// 换车点（StoreId）

        /* 逻辑指向门店 */
        public int? ReturnThePoint { get; set; }// 还车点（StoreId）

        public string UserName { get; set; }// 用户姓名

        public string UserPhone { get; set; }// 用户手机号

        public string? IdentityCard { get; set; }// 身份证号

        public bool LongTermLease { get; set; } = false;// 长租

        [Precision(10, 2)]
        public decimal Deposit { get; set; }// 押金

        [Precision(10, 2)]
        public decimal Rent { get; set; }// 租金

        [Precision(10, 2)]
        public decimal DispatchFee { get; set; } = 0;// 调度费

        [Precision(10, 2)]
        public decimal OtherFees { get; set; } = 0;// 其他费用

        [Precision(10, 2)]
        public decimal Paid { get; set; } = 0;// 已付

        [Precision(10, 2)]
        public decimal DepositRefunded { get; set; } = 0;// 已退押金

        public OrderStatus Status { get; set; }// 订单状态

        public string? Notes { get; set; }// 备注

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public enum OrderStatus {
            已取消,
            待付款,
            已退款,
            待确认,
            进行中,
            已完成
        }
    }
}
