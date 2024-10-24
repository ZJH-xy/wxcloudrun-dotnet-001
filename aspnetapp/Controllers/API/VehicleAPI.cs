namespace aspnetapp.Controllers.API {

    [Route("vehicle")]
    [ApiController]
    public class VehicleAPI : ControllerBase {
        VehicleController vehicleController = new(new MyDbContext());

        // 获取门店所有车辆
        [HttpGet("a/{storeId}")]
        public async Task<IActionResult> Getvehicles(int storeId) {
            List<VehicleBasic> vehicleBasicsList = new();
            try {
                vehicleBasicsList = await vehicleController.GetVehicleByStoreId(storeId);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetStores: {e}");
#endif
                return StatusCode(500);
            }

            return StatusCode(200, vehicleBasicsList);
        }

        // 获取车辆信息
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id) {
            VehiclePro? vehicle = null;
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

            return StatusCode(200, vehicle);
        }
    }
}
