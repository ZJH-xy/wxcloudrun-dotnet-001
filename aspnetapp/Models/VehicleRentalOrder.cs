namespace aspnetapp.Models {
    // 租车订单信息表
    [Table("T_VehicleRentalOrderInformation")]
    public class VehicleRentalOrder {
        [Key]
        [ForeignKey("Order")]
        public int OrderId { get; set; }// 订单编号（OrderId）

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }// 车辆编号（VehicleId）

        public DateTime startingTime { get; set; }// 起始时间

        public DateTime ExpectedReturnTime { get; set; }// 预计归还时间

        public DateTime ActualReturnTime { get; set; }// 实际归还时间

        public int RentalLocation { get; set; }// 租车点（StoreId）

        public int TransferPoint { get; set; }// 换车点（StoreId）

        public int ReturnThePoint { get; set; }// 还车点（StoreId）

    }
}
