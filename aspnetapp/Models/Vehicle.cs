namespace aspnetapp.Models {
    // 车辆总表
    [Table("T_VehicleSummary")]
    public class Vehicle {
        [Key]
        public int VehicleId { get; init; }// 车辆编号

        public Store? Store { get; set; }// 导航属性，指向门店

        public EVehicle Model { get; set; }// 车辆型号

        public string? PlateNumber { get; set; }// 车牌牌号

        public string? FrameNumber { get; set; }// 车架号

        public bool? Certificate { get; set; }// 合格证

        public bool? Invoice { get; set; }// 发票

        public bool? Drivinglicense;// 行驶证

        public DateTime? PurchaseRegistrationTime { get; set; }// 购入登记时间

        public string? Owner { get; set; }// 车主

        public string? VehicleIntroduction { get; set; }// 车辆介绍

        public Estates State { get; set; }// 车辆状态

        public DateTime StateUpdatedAt { get; set; }// 更新时间

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDelete { get; set; } = false;

        public enum Estates {
            Idle,// 空闲
            Leased,// 已出租
            Charging,// 充电中
            Fault// 故障
        }

        // 车辆型号
        public enum EVehicle {
            UnknownOrSecondHand,// 未知或二手
            Case,// 涉案车辆
            Harting,// 雅迪
            Aima,// 爱玛
            Tailg// 台铃
        }
    }
}
