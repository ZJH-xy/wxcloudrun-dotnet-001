namespace aspnetapp.Models {
    // 订单表
    [Table("T_OrderForm")]
    public class Order {
        [Key]
        public int OrderId { get; set; }// 订单编号

        [ForeignKey("Users")]
        public int UserId { get; set; }// 用户编号（UserId）

        public decimal Deposit { get; set; }// 押金

        public decimal Rent { get; set; }// 租金

        public decimal DispatchFee { get; set; }// 调度费

        public decimal OtherFees { get; set; }// 其他费用

        public decimal Paid { get; set; }// 已付

        public decimal DepositRefunded { get; set; }// 已退押金

        public OrderStatus Status { get; set; }// 订单状态

        public string? Notes { get; set; }// 备注

        public enum OrderStatus {
            inProgress,// 进行中
            PendingPayment,// 待付款
            Completed// 已完成
        }
    }
}
