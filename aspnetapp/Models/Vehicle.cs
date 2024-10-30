namespace aspnetapp.Models {
    // 车辆总表
    [Table("T_VehicleSummary")]
    [Index(nameof(OriginalStore), nameof(CurrentStore))]
    public class Vehicle {
        [Key]
        public int VehicleId { get; init; }// 车辆编号

        /* 逻辑指向门店 */
        public int? OriginalStore { get; set; }// 原始门店

        /* 逻辑指向门店 */
        public int? CurrentStore { get; set; }// 当前门店

        public Emodel Model { get; set; } = Emodel.未知;// 车辆型号

        public string? PlateNumber { get; set; }// 车牌牌号

        public string? FrameNumber { get; set; }// 车架号

        public bool? Certificate { get; set; }// 合格证

        public bool? Invoice { get; set; }// 发票

        public bool? Drivinglicense { get; set; }// 行驶证

        public DateTime? PurchaseRegistrationTime { get; set; }// 购入登记时间

        public string? Owner { get; set; }// 车主

        public string? VehicleIntroduction { get; set; }// 车辆介绍

        public string? Pictures { get; set; }// 车辆图片

        public Estates State { get; set; }// 车辆状态

        public bool IsCase { get; set; } = false; // 是否涉案

        public DateTime StateUpdatedAt { get; set; }// 状态更新时间

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool IsDelete { get; set; } = false;

        public enum Estates {
            空闲,
            已出租,
            充电中,
            故障,
            锁定,
            侍确认
        }

        // 车辆型号
        public enum Emodel {
            未知,
            雅迪,
            爱玛,
            台铃
        }
    }
}
