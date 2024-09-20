#nullable disable

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;


namespace aspnetapp.Test {
    [Route("api/test")]
    [ApiController]
    public class Test : ControllerBase {
        public Test() {

        }

        // GET
        [HttpGet]
        public Object GetNumber() {
            return new { success = true, code = 200 };
        }
    }
}