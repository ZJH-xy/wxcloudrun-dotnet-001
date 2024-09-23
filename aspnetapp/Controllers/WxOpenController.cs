using Microsoft.AspNetCore.Mvc;
using Senparc.Weixin;
using Senparc.Weixin.Containers;
using Senparc.Weixin.MP.Containers;
using Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp;

namespace aspnetapp.Controllers {
    [Route("WxOpen/[controller]")]
    [ApiController]
    public class GetUserPhoneNumberController : ControllerBase {
        [HttpGet]
        public JsonResult Get() {
            Console.WriteLine(BaseContainer<AccessTokenBag>.GetFirstOrDefaultAppId(PlatformType.WxOpen));
            return new JsonResult(new { message = "无参数" });
        }

        [HttpGet("{code}")]// GET WxOpen/GetUserPhoneNumber
        public async Task<JsonResult> GetUserPhoneNumber(string code) {
            try {
                //await AccessTokenContainer.RegisterAsync("wx64a0621808b2e3c7", "cc3d71ae4452bd89dc83d10f934e8a20");
                
                var result = await BusinessApi.GetUserPhoneNumberAsync(BaseContainer<AccessTokenBag>.GetFirstOrDefaultAppId(PlatformType.WxOpen), code);
                return new JsonResult(new { message = result });
            } catch (Exception ex) {
                return new JsonResult(new { message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Post() {
            return new JsonResult(new { message = "111" });
        }
    }
}