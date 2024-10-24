namespace aspnetapp.Controllers.API {

    [Route("store")]
    [ApiController]
    public class StoreAPI : ControllerBase {
        StoreController storeController = new(new MyDbContext());

        // 获取所有门店
        [HttpGet("a")]
        public async Task<IActionResult> GetStores() {
            try {
                List<StoreBasic> storeBasicList = await storeController.GetAllStore();
                return StatusCode(200, new { storeBasicList });

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetStores: {e}");
#endif
                return StatusCode(500);
            }
        }
    }
}
