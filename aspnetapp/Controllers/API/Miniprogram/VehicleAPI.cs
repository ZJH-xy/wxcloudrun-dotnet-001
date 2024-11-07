using aspnetapp.Controllers.Miniprogram;

namespace aspnetapp.Controllers.API.Miniprogram
{

    [Route("vehicle")]
    [ApiController]
    public class VehicleAPI : ControllerBase {
        private readonly VehicleController vehicleController = new(new MyDbContext());
        private readonly ILogger<OrderAPI> _logger;

        public VehicleAPI(ILogger<OrderAPI> logger) {
            _logger = logger;
        }

        // Id获取门店所有车辆
        [HttpGet("a/{storeId}")]
        public async Task<IActionResult> Getvehicles(int storeId) {
            List<Vehicle> vehicleList;
            try {
                vehicleList = await vehicleController.GetVehicleByStoreId(storeId);

            } catch (Exception e) {
                _logger.LogError(e, "获取门店{StoreId}所有车辆", storeId);

                return StatusCode(500);
            }

            List<VehicleBasic> vehiclesBasicsList = new();

            foreach (Vehicle vehicle in vehicleList) {
                vehiclesBasicsList.Add(new VehicleBasic(vehicle));
            }

            return StatusCode(200, vehiclesBasicsList);
        }

        // Id获取车辆信息
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id) {
            Vehicle? vehicle;
            try {
                vehicle = await vehicleController.GetVehicleById(id);

            } catch (Exception e) {
                _logger.LogError(e, "获取车辆{VehicleId}信息", id);

                return StatusCode(500);
            }

            if (vehicle is null)
                return StatusCode(404);

            return StatusCode(200, new VehiclePro(vehicle));
        }

        // 查询车辆model、车牌
        [HttpGet("model/{id}")]
        public async Task<IActionResult> GetVehicleModelById(int id) {
            object? vehicle;
            try {
                vehicle = await vehicleController.GetVehicleQueryableById(id);

            } catch (Exception e) {
                _logger.LogError(e, "查询车辆{VehicleId}", id);

                return StatusCode(500);
            }

            if (vehicle is null)
                return StatusCode(404);

            return StatusCode(200, vehicle);
        }
    }
    public struct VehicleBasic {
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
