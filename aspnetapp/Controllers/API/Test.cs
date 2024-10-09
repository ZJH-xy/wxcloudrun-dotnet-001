#nullable disable

using aspnetapp;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;


namespace aspnetapp.Controllers.API
{
    [Route("api/test")]
    [ApiController]
    public class Test : ControllerBase
    {
        public Test()
        {

        }

        // GET
        [HttpGet]
        public object GetNumber()
        {
            return new { success = true, code = 200 };
        }
    }
}