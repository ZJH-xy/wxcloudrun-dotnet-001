namespace aspnetapp.Controllers.API {

    [Route("vehicle")]
    [ApiController]
    public class VehicleAPI : ControllerBase {
        VehicleController vehicleController = new(new MyDbContext());

        // 获取门店所有车辆
        [HttpGet("a/{storeId}")]
        public async Task<IActionResult> Getvehicles(int storeId) {
            List<Vehicle> vehicleList = new();
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
            Vehicle? vehicle = null;
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
    }
}
