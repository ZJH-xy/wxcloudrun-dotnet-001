namespace aspnetapp.Controllers.API {

    [Route("vehicle")]
    [ApiController]
    public class VehicleAPI : ControllerBase {
        VehicleController vehicleController = new(new MyDbContext());

        // 获取门店所有车辆
        [HttpGet("a/{storeId}")]
        public async Task<IActionResult> Getvehicles(int storeId) {
            List<Vehicle> vehicleList;
            try {
                vehicleList = await vehicleController.GetVehicleByStoreId(storeId);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetStores: {e}");
#endif
                return StatusCode(500);
            }

            List<VehicleBasic> vehiclesBasicsList = new();

            foreach (Vehicle vehicle in vehicleList) {
                vehiclesBasicsList.Add(new VehicleBasic(vehicle));
            }

            return StatusCode(200, vehiclesBasicsList);
        }

        // 获取车辆信息
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id) {
            Vehicle? vehicle;
            try {
                vehicle = await vehicleController.GetVehicleById(id);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetUserById: {e}");
#endif
                return StatusCode(500);
            }

            if (vehicle is null)
                return StatusCode(404);

            return StatusCode(200, new VehiclePro(vehicle));
        }

        // 查询车辆模型、车牌
        [HttpGet("model/{id}")]
        public async Task<IActionResult> GetVehicleModelById(int id) {
            object? vehicle;
            try {
                vehicle = await vehicleController.GetVehicleQueryableById(id);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetVehicleModelById: {e}");
#endif
                return StatusCode(500);
            }

            if (vehicle is null)
                return StatusCode(404);

            return StatusCode(200, vehicle);
        }
    }
    public struct VehicleBasic {
        public VehicleBasic(Vehicle store) {
            VehicleId = store.VehicleId;
            Model = store.Model;
            State = store.State;
        }

        public int VehicleId { get; init; }// 车辆编号
        public Vehicle.Estates State { get; set; }// 车辆状态
        public Vehicle.Emodel Model { get; set; } = Vehicle.Emodel.未知;// 车辆型号
    }

    public struct VehiclePro {
        public VehiclePro(Vehicle store) {
            VehicleId = store.VehicleId;
            Model = store.Model;
            PlateNumber = store.PlateNumber;
            FrameNumber = store.FrameNumber;
            VehicleIntroduction = store.VehicleIntroduction;
            State = store.State;
        }

        public int VehicleId { get; init; }// 车辆编号
        public Vehicle.Estates State { get; set; }// 车辆状态
        public Vehicle.Emodel Model { get; set; } = Vehicle.Emodel.未知;// 车辆型号
        public string? PlateNumber { get; set; }// 车牌牌号
        public string? FrameNumber { get; set; }// 车架号
        public string? VehicleIntroduction { get; set; }// 车辆介绍
    }
}
