namespace aspnetapp.Controllers.API {

    [Route("vehicle")]
    [ApiController]
    public class VehicleAPI : ControllerBase {
        VehicleController vehicleController = new VehicleController(new MyDbContext());

        [HttpGet("a/{storeId}")]
        public async Task<IActionResult> Getvehicles(int storeId) {
            try {
                return StatusCode(200, await vehicleController.GetVehicleByStoreId(storeId));
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetStores: {e}");
#endif
                return StatusCode(500);
            }
        }
    }
}
