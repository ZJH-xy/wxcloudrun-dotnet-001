using aspnetapp.Controllers.Miniprogram;
using aspnetapp.Models;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;

namespace aspnetapp.Controllers.API.Miniprogram
{

    [Route("vehicle")]
    [ApiController]
    public class VehicleAPI : ControllerBase {
        private readonly MyDbContext _dbContext;
        private readonly ILogger<VehicleAPI> _logger;
        private readonly VehicleController _vehicleController;
		private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

		public VehicleAPI(MyDbContext dbContext, ILogger<VehicleAPI> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            _dbContext = dbContext;
            _logger = logger;
            _vehicleController = new(_dbContext);
			_wxSetting = wxSetting;
		}

        /// <summary>
        /// Id获取门店所有车辆
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [HttpGet("a/{storeId}")]
        public async Task<IActionResult> Getvehicles(int storeId) {
            List<Vehicle> vehicleList;
            try {
                vehicleList = await _vehicleController.GetVehicleByStoreId(storeId);

            } catch (Exception e) {
                _logger.LogError(e, "获取门店{StoreId}所有车辆", storeId);

                return StatusCode(500);
			}

            List<VehicleBasic> vehiclesBasicsList = new();
			List<int> downloadPositionList = new();// 有图片的位置
			List<FileItem> fileidList = new();// 文件下载链接的列表

			for (int i = 0; i < vehicleList.Count; ++i) {
				vehiclesBasicsList.Add(new(vehicleList[i]));
				if (!vehicleList[i].Pictures.IsNullOrEmpty()) {
					// 如有图片则放入列表
					fileidList.Add(new FileItem {
						fileid = vehicleList[i].Pictures,
						max_age = 7200
					});
					downloadPositionList.Add(i);
				}
			}

            if (fileidList.Count > 0) {
				// 下载链接
				Result_File_List[] downloadLinkArray = await GetImageDownload(fileidList);

				for (int i = 0; i < downloadLinkArray.Length; ++i) {
					vehiclesBasicsList[downloadPositionList[i]].Pictures = downloadLinkArray[i].download_url;
				}
			}

            return StatusCode(200, vehiclesBasicsList);
        }

        /// <summary>
        /// Id获取车辆信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id) {
            Vehicle? vehicle;
            try {
                vehicle = await _vehicleController.GetVehicleById(id);

            } catch (Exception e) {
                _logger.LogError(e, "获取车辆{VehicleId}信息", id);

                return StatusCode(500);
			}

            if (vehicle is null)
                return StatusCode(404);

            return StatusCode(200, new VehiclePro(vehicle));
        }

        /// <summary>
        /// 查询车辆model、车牌
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("model/{id}")]
        public async Task<IActionResult> GetVehicleModelById(int id) {
            object? vehicle;
            try {
                vehicle = await _vehicleController.GetVehicleQueryableById(id);

            } catch (Exception e) {
                _logger.LogError(e, "查询车辆{VehicleId}", id);

                return StatusCode(500);
			}

            if (vehicle is null)
                return StatusCode(404);

            return StatusCode(200, vehicle);
		}

		/// <summary>
		/// 获取图片文件下载链接（多个）
		/// </summary>
		/// <param name="fileidList"></param>
		/// <returns></returns>
		private async Task<Result_File_List[]> GetImageDownload(List<FileItem> fileidList) {
			var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
			var envId = _wxSetting.Value.Env;

			var re = await TcbApi.BatchDownloadFileAsync(appId, envId, fileidList);

			if (re.errcode != ReturnCode.请求成功) {
				_logger.LogError("{errmsg},获取下载链接失败{fileidList}", re.errmsg, fileidList.ToJson());
			}

			return re.file_list;
		}

		/// <summary>
		/// 获取图片文件下载链接
		/// </summary>
		/// <param name="userId"></param>
		/// <returns></returns>
		private async Task<string> GetImageDownload(string fileid) {
			var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
			//var appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
			var envId = _wxSetting.Value.Env;

			List<FileItem> fileid_list = new() {
				new FileItem {
					fileid = fileid,
					max_age = 7200
				}
				};

			var re = await TcbApi.BatchDownloadFileAsync(appId, envId, fileid_list);

			if (re.errcode != ReturnCode.请求成功) {
				_logger.LogError("{errmsg},获取下载链接{fileid_list}", re.errmsg, fileid_list.ToJson());
			}

			return re.file_list.First().download_url;
		}
	}
    public class VehicleBasic {
        public VehicleBasic(Vehicle store) {
            VehicleId = store.Id;
            Model = store.Model;
            State = store.State;
            Pictures = store.Pictures;
        }

        public int VehicleId { get; init; }// 车辆编号
        public Vehicle.Estates State { get; set; }// 车辆状态
        public Vehicle.Emodel Model { get; set; } = Vehicle.Emodel.未知;// 车辆型号
        public string? Pictures { get; set; }// 车辆图片
    }

    public struct VehiclePro {
        public VehiclePro(Vehicle store) {
            VehicleId = store.Id;
            Model = store.Model;
            PlateNumber = store.PlateNumber;
            FrameNumber = store.FrameNumber;
            VehicleIntroduction = store.VehicleIntroduction;
            State = store.State;
            Pictures = store.Pictures;
        }

        public int VehicleId { get; init; }// 车辆编号
        public Vehicle.Estates State { get; set; }// 车辆状态
        public Vehicle.Emodel Model { get; set; } = Vehicle.Emodel.未知;// 车辆型号
        public string? PlateNumber { get; set; }// 车牌牌号
        public string? FrameNumber { get; set; }// 车架号
        public string? VehicleIntroduction { get; set; }// 车辆介绍
        public string? Pictures { get; set; }// 车辆图片
    }
}
