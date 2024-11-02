namespace aspnetapp.Models {
    /// <summary>
    /// 换车记录表
    /// </summary>
    [Table("T_VehicleReplacementRecord")]
    [Index(nameof(TheOrder))]
    public class VehicleReplacementRecord {
        [Key]
        public int Id { get; set; }

        /* 逻辑指向订单 */
        public int TheOrder { get; set; }

        /* 逻辑指向车辆 */
        public int TheOldVehicles { get; set; }// 旧车辆

        /* 逻辑指向车辆 */
        public int TheNewVehicles { get; set; }// 新车辆

        public Estates State { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public enum Estates {
            已取消,
            侍确认,
            已完成
        }
    }
}
