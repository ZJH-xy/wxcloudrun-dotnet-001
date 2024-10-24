namespace aspnetapp.Controllers.API {

    [Route("store")]
    [ApiController]
    public class StoreAPI : ControllerBase {
        StoreController storeController = new(new MyDbContext());

        // 获取所有门店
        [HttpGet("a")]
        public async Task<IActionResult> GetStores() {
            List<Store> storeList = new();
            try {
                storeList = await storeController.GetAllStore();

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetStores: {e}");
#endif
                return StatusCode(500);
            }

            List<StoreBasic> storeBasicList = new();

            foreach (Store store in storeList) {
                storeBasicList.Add(new StoreBasic(store));
            }

            return StatusCode(200, storeBasicList);
        }
    }

    public struct StoreBasic {
        public StoreBasic(Store store) {
            StoreId = store.StoreId;
            Name = store.Name;
            BusinessHoursStart = store.BusinessHoursStart;
            BusinessHoursBegin = store.BusinessHoursBegin;
            BusinessStatus = store.BusinessStatus;
            Telephone = store.Telephone;
            Address = store.Address;
            GpsLongitude = store.GpsLongitude;
            GpsLatitude = store.GpsLatitude;
            Pictures = store.Pictures;
            Introduce = store.Introduce;
        }

        public int StoreId { get; init; }// 门店编号

        public string Name { get; set; } = string.Empty;// 门店名称

        public TimeSpan BusinessHoursStart { get; set; }// 营业开始时间

        public TimeSpan BusinessHoursBegin { get; set; }// 营业结束时间

        public bool BusinessStatus { get; set; }// 营业状态

        public string? Telephone { get; set; }// 联系电话

        public string? Address { get; set; }// 门店地址

        public double GpsLongitude { get; init; }// Longitude 经度，范围 [-180, 180]

        public double GpsLatitude { get; init; }// Latitude 纬度，范围 [-90, 90]

        public string? Pictures { get; set; }// 门店图片

        public string? Introduce { get; set; }// 介绍
    }
}
