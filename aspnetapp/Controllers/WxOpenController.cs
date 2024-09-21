using Microsoft.AspNetCore.Mvc;
using Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp;
using Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp.Business.JsonResult;

namespace aspnetapp.Controllers {

    [Route("/WxOpen/[controller]")]
    [ApiController]
    public class GetUserPhoneNumberController {

        public class Login : ControllerBase {

            // GET
            [HttpGet("{code}")]
            public async Task<Info> GetUserPhoneNumber(string code) {
                
                try {
                    var result = await BusinessApi.GetUserPhoneNumberAsync(DEF.WxOpenAppId, code);
                    return new ScInfo(200, result.phone_info);
                    //return new { success = true, phoneInfo = result.phone_info };
                } catch (Exception ex) {
                    return new ExInfo(401, ex.Message);
                    //return new { success = false, msg = ex.Message };
                }
            }
        }
    }


    public record Info(int Code);
    public record ScInfo(int Code, Object? Message) : Info(Code);
    public record ExInfo(int Code, string? Message) : Info(Code);
}
