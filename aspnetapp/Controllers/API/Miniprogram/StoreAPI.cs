using aspnetapp.Controllers.Miniprogram;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace aspnetapp.Controllers.API.Miniprogram {

    [Route("store")]
    [ApiController]
    public class StoreAPI : ControllerBase {
        private readonly MyDbContext _dbContext;
        private readonly ILogger<StoreAPI> _logger;
        private readonly StoreController _storeController;
        private readonly FavoritesStoreController _favoritesStoreController;

        public StoreAPI(MyDbContext dbContext, ILogger<StoreAPI> logger) {
            _dbContext = dbContext;
            _logger = logger;
            _storeController = new(_dbContext);
            _favoritesStoreController = new(_dbContext);
        }

        /// <summary>
        /// 获取门店名称
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("name/{id}")]
        public async Task<IActionResult> GetStoreName(int id) {
            Store? store;
            try {
                store = await _dbContext.Store.FirstOrDefaultAsync(s => s.Id == id);

            } catch (Exception e) {
                _logger.LogError(e, "获取门店{StoreId}名称", id);
                return StatusCode(500);
				
			}

            if (store is null)
                return StatusCode(404);

            return StatusCode(200, store.Name);
        }

        /// <summary>
        /// 获取所有门店
        /// </summary>
        /// <returns></returns>
        [HttpGet("a")]
        public async Task<IActionResult> GetStores() {
            List<Store> storeList;
            try {
                storeList = await _storeController.GetAllStore();

            } catch (Exception e) {
                _logger.LogError(e, "获取所有门店");

                return StatusCode(500);
				
			}

            List<StoreBasic> storeBasicList = new();

            foreach (Store store in storeList) {
                storeBasicList.Add(new StoreBasic(store));
            }

            return StatusCode(200, storeBasicList);
        }

        /// <summary>
        /// 获取门店套餐
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [HttpGet("storeMenus/{storeId}")]
        public async Task<IActionResult> CalculateRent(int storeId) {

            return StatusCode(200, await _dbContext.StoreMenus.Where(sm => sm.TheStore == storeId && !sm.IsDelete).ToListAsync());
        }

        /// <summary>
        /// 收藏门店
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost("favorites/{storeId}")]
        public async Task<IActionResult> AddFavoritesStore(int storeId) {
            List<UserFavoritesStore> list = await _favoritesStoreController.GetByUserId(GetUserIdInt());

            /*foreach (UserFavoritesStore store in list) {
                if (store.Id == storeId) {
                    return StatusCode(403, "不可重复收藏");
                }
            }*/

            if (list.Any(s => s.TheStore == storeId))
                return StatusCode(403, "不可重复收藏");

            await _favoritesStoreController.AddFavoritesStore(GetUserIdInt(), storeId);
            return StatusCode(200);
        }

        /// <summary>
        /// JWT 获取用户id
        /// </summary>
        /// <returns></returns>
        public int GetUserIdInt() {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }

    public struct StoreBasic {
        public StoreBasic(Store store) {
            StoreId = store.Id;
            Name = store.Name;
            BusinessHoursStart = store.BusinessHoursStart;
            BusinessHoursBegin = store.BusinessHoursEnd;
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
