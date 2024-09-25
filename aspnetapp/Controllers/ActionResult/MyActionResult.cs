using Microsoft.AspNetCore.Mvc;

// 不使用，仅参考
namespace aspnetapp.Controllers.ActionResult {
    public class MyActionResult : IActionResult {
        public bool success { get; set; }
        public int code { get; set; }
        public string message { get; set; } = "无消息";

        public JsonResult dataJson { get; set; }

        public MyActionResult(int code, string message, bool success, JsonResult data) {
            this.code = code;
            this.message = message;
            this.success = success;
            this.dataJson = data;
        }
        public Task ExecuteResultAsync(ActionContext context) {
            throw new NotImplementedException();
        }
    }
}
