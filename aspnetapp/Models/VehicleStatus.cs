namespace aspnetapp.Models {
    // 车辆状态表
    [Table("T_VehicleStatus")]
    public class VehicleStatus {
        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }// 车辆编号（VehicleId）

        public Estates State { get; set; }// 车辆状态

        public DateTime UpdatedAt { get; set; }// 更新时间

        public enum Estates {
            Idle,// 空闲
            Leased,// 已出租
            Charging,// 充电中
            Fault// 故障
        }
    }
}
