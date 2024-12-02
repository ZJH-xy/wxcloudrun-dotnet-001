using aspnetapp.Controllers.API.Miniprogram;
using aspnetapp.Controllers.Miniprogram;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.TenPayV3.Apis.BasePay.Entities;
using Senparc.Weixin.TenPayV3;
using Senparc.CO2NET.Extensions;
using System.Collections;

namespace aspnetapp.Pages.Callback {
	public class RefundModel : PageModel {
		private readonly MyDbContext _dbContext;
		private readonly ILogger<RefundModel> _logger;
		private readonly OrderController _orderController;

		public RefundModel(MyDbContext dbContext, ILogger<RefundModel> logger) {
			_dbContext = dbContext;
			_logger = logger;
			_orderController = new(_dbContext);
		}

		public IActionResult OnGet() {
			return StatusCode(200);
		}

		#region 退款回调
		public async Task<IActionResult> OnPostAsync() {
			WeixinTrace.SendCustomLog("RefundNotifyUrl被访问", "IP" + HttpContext.UserHostAddress()?.ToString());

			WxPayCallbackViewModel returnData = new();
			try {
				var resHandler = new TenPayNotifyHandler(HttpContext);
				var refundNotifyJson = await resHandler.DecryptGetObjectAsync<RefundNotifyJson>();


				WeixinTrace.SendCustomLog("跟踪RefundNotifyUrl信息", refundNotifyJson.ToJson(true));

				string refund_status = refundNotifyJson.refund_status;
				//if (refundNotifyJson.VerifySignSuccess != true)
				if (/*refundNotifyJson.VerifySignSuccess == true &*/ refund_status == "SUCCESS") {
					//returnData.code = "SUCCESS";
					//returnData.message = "OK";

					//填写逻辑
					WeixinTrace.SendCustomLog("RefundNotifyUrl被访问", "验证通过");
					_logger.LogInformation("RefundNotifyUrl被访问，验证通过");

					//获取接口中需要用到的信息 例
					//string transaction_id = refundNotifyJson.transaction_id;
					//string out_trade_no = refundNotifyJson.out_trade_no;
					//string refund_id = refundNotifyJson.refund_id;
					//string out_refund_no = refundNotifyJson.out_refund_no;
					//int total_fee = refundNotifyJson.amount.payer_total;
					//int refund_fee = refundNotifyJson.amount.refund;

					var refundOrder = await _dbContext.RefundOrder.SingleAsync(ro => ro.Id == int.Parse(refundNotifyJson.out_refund_no));
					refundOrder.Status = (RefundOrder.Estatus)RefundOrderEstatusHashtable[refundNotifyJson.refund_status]!;
					refundOrder.SuccessTime = DateTimeOffset.Parse(refundNotifyJson.success_time).UtcDateTime;// 退款成功时间
					refundOrder.UpdatedAt = DateTime.Now;

					try {
						await _dbContext.SaveChangesAsync();

					} catch (Exception e) {
						_logger.LogError(e, "更新退款refundOrder：{refundOrder}", System.Text.Json.JsonSerializer.Serialize(refundOrder));
						returnData.code = "FAILD";
						returnData.message = "数据库更新错误";
						return StatusCode(500, returnData);
						throw;
					}

					_logger.LogInformation("refundOrder更新{refundOrderId}", refundOrder.Id);

					return StatusCode(200);
				} else {
					returnData.code = "FAILD";
					returnData.message = "验证失败";
					WeixinTrace.SendCustomLog("RefundNotifyUrl被访问", "验证失败");
					_logger.LogInformation("RefundNotifyUrl被访问，验证失败");

					return StatusCode(400, returnData);
				}

				//进行后续业务处理

			} catch (Exception ex) {
				returnData.code = "FAILD";
				returnData.message = ex.Message;
				WeixinTrace.WeixinExceptionLog(new WeixinException(ex.Message, ex));
			}

			//https://pay.weixin.qq.com/wiki/doc/apiv3/wechatpay/wechatpay3_3.shtml
			//return Json(returnData);

			return StatusCode(200);
		}
		#endregion

		/*【退款状态】退款到银行发现用户的卡作废或者冻结了，导致原路退款银行卡失败，可前往商户平台（pay.weixin.qq.com）-交易中心，手动处理此笔退款。
		* SUCCESS: 退款成功
		* CLOSED: 退款关闭
		* PROCESSING: 退款处理中
		* ABNORMAL: 退款异常
		*/
		/// <summary>
		/// 退款状态Hashtable，与回调共用！！！
		/// </summary>
		public Hashtable RefundOrderEstatusHashtable = new() {
			{ "SUCCESS", RefundOrder.Estatus.退款成功 },
			{ "CLOSED", RefundOrder.Estatus.退款关闭 },
			{ "PROCESSING", RefundOrder.Estatus.退款处理中 },
			{ "ABNORMAL", RefundOrder.Estatus.退款异常 }
		};
	}
}
