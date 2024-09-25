namespace aspnetapp.Models {
    // 门店总表
    [Table("T_StoreSummary")]
    public class StoreVehicle {
        [Key]
        public int StoreId { get; set; }// 门店编号

        [Required(ErrorMessage = "门店名称不可为空")]
        public string Name { get; set; } = string.Empty;// 门店名称

        [DataType(DataType.Time)]// HH:MM:SS
        [Required(ErrorMessage = "营业开始时间不可为空")]
        public DateTime BusinessHoursStart { get; set; }// 营业开始时间

        [DataType(DataType.Time)]// HH:MM:SS
        [Required(ErrorMessage = "营业结束时间不可为空")]
        public DateTime BusinessHoursBegin { get; set; }// 营业结束时间

        public bool BusinessStatus { get; set; } = true;// 营业状态

        public string? Telephone { get; set; }// 联系电话

        public string? WeChat { get; set;}// 微信

        public string? Address { get; set; }// 门店地址

        public string? GPS { get; set; }// 经纬度

        public string? Pictures { get; set; }// 门店图片

        public string? Introduce { get; set; }// 介绍

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
