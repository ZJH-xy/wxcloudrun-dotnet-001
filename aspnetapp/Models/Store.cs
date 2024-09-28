namespace aspnetapp.Models {
    // 门店总表
    [Table("T_StoreSummary")]
    public class Store {
        [Key]
        public int StoreId { get; init; }// 门店编号

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

        public Geo? Gps { get; set; }// 经纬度

        public string? Pictures { get; set; }// 门店图片

        public string? Introduce { get; set; }// 介绍

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();// 一个门店有多个车辆

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        
        public bool IsDelete { get; set; } = false;

        public readonly struct Geo {
            public double Longitude { get; init; }// Longitude 经度，范围 [-180, 180]

            public double Latitude { get; init; }// Latitude 纬度，范围 [-90, 90]

            // 构造函数初始化
            public Geo(double longitude, double latitude) {
                if (longitude < -180 || longitude > 180) {
                    throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
                }
                if (latitude < -90 || latitude > 90) {
                    throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
                }

                this.Longitude = longitude;
                this.Latitude = latitude;
            }
        }
    }
}
