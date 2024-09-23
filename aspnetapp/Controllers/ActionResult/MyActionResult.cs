using Microsoft.AspNetCore.Mvc;

namespace aspnetapp.Controllers.ActionResult {
    public class MyActionResult : IActionResult {
        public int code { get; set; }
        public string message { get; set; } = "无消息";

        public MyActionResult(int code, string message) {
            this.code = code;
            this.message = message;
        }
        public Task ExecuteResultAsync(ActionContext context) {
            throw new NotImplementedException();
        }
    }
}
