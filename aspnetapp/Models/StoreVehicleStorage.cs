namespace aspnetapp.Models {
    // 门店存放车辆表
    [Table("T_StoreStorageVehicleList")]
    public class StoreStorageVehicleList {
        [ForeignKey("StoreVehicle")]
        public int StoreId { get; set; }// 门店编号（StoreId）

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }// 车辆编号（VehicleId）

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
