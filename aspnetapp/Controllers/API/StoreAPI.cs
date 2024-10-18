using Senparc.CO2NET;

namespace aspnetapp.Controllers.API {
    [Route("store")]
    [ApiBind]
    public class StoreAPI : ControllerBase {
        StoreController storeController = new StoreController(new MyDbContext());

        [HttpGet("a")]
        public async Task<IActionResult> GetStores() {
            try {
                List<StoreBasic> storeBasic = await storeController.GetAllOrders();

                return StatusCode(200, new { store_basic = storeBasic });
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetStores: {e}");
#endif
                return StatusCode(500, "服务器错误");
            }
        }
    }
}
