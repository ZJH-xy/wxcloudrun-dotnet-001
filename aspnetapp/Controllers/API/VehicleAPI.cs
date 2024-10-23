namespace aspnetapp.Controllers.API {

    [Route("vehicle")]
    [ApiController]
    public class VehicleAPI : ControllerBase {
        VehicleController vehicleController = new VehicleController(new MyDbContext());

        [HttpGet("a/{storeId}")]
        public async Task<IActionResult> Getvehicles(int storeId) {
            try {
                List<VehicleBasic> vehiclesBasicsList = await vehicleController.GetVehicleByStoreId(storeId);

                return StatusCode(200, new { vehiclesBasicsListJson = vehiclesBasicsList.ToJson() });
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetStores: {e}");
#endif
                return StatusCode(500, "服务器错误");
            }
        }
    }
}
