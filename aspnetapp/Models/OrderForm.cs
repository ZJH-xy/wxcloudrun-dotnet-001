using System.Runtime.InteropServices;

namespace aspnetapp.Models {
    // 订单表
    [Table("T_OrderForm")]
    public class Order {
        [Key]
        public int OrderId { get; init; }// 订单编号

        [ForeignKey("User")]
        public int UserId { get; set; }// 用户编号（UserId）
        public User? user { get; set; }

        public DateTime StartingTime { get; set; }// 起始时间

        public DateTime ExpectedReturnTime { get; set; }// 预计归还时间

        public DateTime ActualReturnTime { get; set; }// 实际归还时间

        public int RentalLocation { get; set; }// 租车点（StoreId）

        public int TransferPoint { get; set; }// 换车点（StoreId）

        public int ReturnThePoint { get; set; }// 还车点（StoreId）

        public bool? LongTermLease { get; set; }// 长租

        public decimal Deposit { get; set; }// 押金

        public decimal Rent { get; set; }// 租金

        public decimal DispatchFee { get; set; }// 调度费

        public decimal OtherFees { get; set; }// 其他费用

        public decimal Paid { get; set; }// 已付

        public decimal DepositRefunded { get; set; }// 已退押金

        public OrderStatus Status { get; set; }// 订单状态

        public string? Notes { get; set; }// 备注

        public enum OrderStatus {
            Cancelled,// 已取消
            InProgress,// 进行中
            PendingPayment,// 待付款
            Completed// 已完成
        }
    }
}
