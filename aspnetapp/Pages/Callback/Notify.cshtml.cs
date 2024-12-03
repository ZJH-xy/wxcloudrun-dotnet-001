using aspnetapp.Controllers.API.Miniprogram;
using aspnetapp.Controllers.Miniprogram;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Senparc.CO2NET.Utilities;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.TenPayV3.Apis.BasePay;
using Senparc.Weixin.TenPayV3.Apis;
using Senparc.Weixin.TenPayV3;
using Senparc.CO2NET.Extensions;

namespace aspnetapp.Pages.Callback {
	public class NotifyModel : PageModel {
		private readonly MyDbContext _dbContext;
		private readonly ILogger<NotifyModel> _logger;
		private readonly OrderController _orderController;

		public NotifyModel(MyDbContext dbContext, ILogger<NotifyModel> logger) {
			_dbContext = dbContext;
			_logger = logger;
			_orderController = new(_dbContext);
		}

		public IActionResult OnGet() {
			return StatusCode(200);
		}

		#region 接收支付回调
		/// <summary>
		/// 支付回调
		/// </summary>
		/// <returns></returns>
		public async Task<IActionResult> OnPostAsync() {
			WxPayCallbackViewModel returnData = new();// 创建应答格式
			try {
				//获取微信服务器异步发送的支付通知信息
				TenPayNotifyHandler resHandler = new TenPayNotifyHandler(HttpContext);
				OrderReturnJson orderReturnJson = await resHandler.DecryptGetObjectAsync<OrderReturnJson>();

				//记录日志
				_logger.LogDebug("PayNotifyUrl收到微信支付回调：{data}", orderReturnJson.ToJson(true));
				Senparc.Weixin.WeixinTrace.SendCustomLog("PayNotifyUrl 接收到消息", orderReturnJson.ToJson(true));

				//演示记录 transaction_id，实际开发中需要记录到数据库，以便退款和后续跟踪
				// transaction_id 微信支付系统生成的订单号。
				//Order? order = await _orderController.GetById(GetUserIdInt(), int.Parse(orderReturnJson.out_trade_no));// 根据Id获取对应的订单
				//Order order = await _dbContext.Order.SingleAsync(o => o.Id.ToString() == orderReturnJson.out_trade_no);
				Order order = await _dbContext.Order.SingleAsync(o => ("TEST" +　o.Id.ToString()) == orderReturnJson.out_trade_no);

				if (order is null) {
					_logger.LogError("订单获取错误transaction_id：{transaction_id}", orderReturnJson.out_trade_no);
					throw new Exception("订单获取错误transaction_id");
				}
				var now = DateTime.Now;
				order.TransactionId = orderReturnJson.transaction_id;// 赋值微信传入的id
				order.UpdatedAt = now;

				//获取支付状态
				string trade_state = orderReturnJson.trade_state;

				//验证请求是否从微信发过来（安全）

				//验证可靠的支付状态
				if (orderReturnJson.VerifySignSuccess == true) {
					/* 交易状态，枚举值：
					 * SUCCESS：支付成功
					 * REFUND：转入退款
					 * NOTPAY：未支付
					 * CLOSED：已关闭
					 * REVOKED：已撤销（付款码支付）
					 * USERPAYING：用户支付中（付款码支付）
					 * PAYERROR：支付失败(其他原因，如银行返回失败)
					 */
					switch (trade_state) {
						case "SUCCESS":
							/* 推荐的做法是，当商户系统收到通知进行处理时，先检查对应业务数据的状态，并判断该通知是否已经处理。
							 * 如果未处理，则再进行处理；如果已处理，则直接返回结果成功。
							 * 在对业务数据进行状态检查和处理之前，要采用数据锁进行并发控制，以避免函数重入造成的数据混乱。
							*/
							if (order.Status == Order.EOrderStatus.待确认) {
								return StatusCode(200);
							}

							// 微信支付订单号查询订单，二次验证
							BasePayApis basePayApis = new();
							string mchid = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId;
							var trade = await basePayApis.OrderQueryByTransactionIdAsync(new QueryRequestData(mchid, order.TransactionId));

							if (trade.trade_state != "SUCCESS") {
								returnData.code = "FAIL";//错误的订单处理
								returnData.message = "订单状态不一致";
							}

							/* 提示：
							* 1、直到这里，才能认为交易真正成功了，可以进行数据库操作，但是别忘了返回规定格式的消息！
							* 2、上述判断已经具有比较高的安全性以外，还可以对访问 IP 进行判断进一步加强安全性。
							* 3、下面演示的是发送支付成功的模板消息提示，非必须。
							*/
							using (var transaction = await _dbContext.Database.BeginTransactionAsync()) {
								try {
									order.Status = Order.EOrderStatus.待确认;// 更改订单状态
									order.Paid += orderReturnJson.amount.total;// 增加已付金额
									order.UpdatedAt = now;

									Vehicle vehicle = await _dbContext.Vehicle.SingleAsync(v => v.Id == order.TheVehicle);
									vehicle.State = Vehicle.Estates.已出租;
									vehicle.StateUpdatedAt = now;

									await _dbContext.SaveChangesAsync();
									await transaction.CommitAsync();
								} catch (Exception e) {
									_logger.LogCritical(e, "支付回调{orderReturnJson}", orderReturnJson.ToJson(true));
									await transaction.RollbackAsync();

									returnData.code = "FAIL";//错误的订单处理
									returnData.message = "服务器错误";
									return StatusCode(403, returnData);
									throw;
								}
							}
							break;

						case "CLOSED":// 已关闭
							using (var transaction = await _dbContext.Database.BeginTransactionAsync()) {
								try {
									order.Status = Order.EOrderStatus.已取消;
									order.UpdatedAt = now;

									Vehicle vehicle = await _dbContext.Vehicle.SingleAsync(v => v.Id == order.TheVehicle);
									vehicle.State = Vehicle.Estates.空闲;
									vehicle.StateUpdatedAt = now;

									await _dbContext.SaveChangesAsync();

									await transaction.CommitAsync();

								} catch (Exception e) {
									_logger.LogError(e, "支付回调{orderReturnJson}", orderReturnJson.ToJson(true));
									await transaction.RollbackAsync();

									returnData.code = "FAIL";//错误的订单处理
									returnData.message = "服务器错误";
									return StatusCode(500, returnData);
									throw;
								}
							}
							break;

						case "PAYERROR":// 支付失败(其他原因，如银行返回失败)


							break;

						default:
							_logger.LogInformation("支付发生其他状态{trade_state}", trade_state);
							//order.Status = order.CreatedAt > now.AddMinutes(10) ? Order.EOrderStatus.已取消 : Order.EOrderStatus.待付款;
							//order.UpdatedAt = now;

							break;
					}
				} else {
					returnData.code = "FAIL";//错误的订单处理
					returnData.message = "验证失败";

					//Order order = await _dbContext.Order.SingleAsync(o => o.Id.ToString() == orderReturnJson.out_trade_no);// 根据Id获取对应的订单
					//order.Status = Order.EOrderStatus.待付款;

					//此处可以给用户发送支付失败提示等
					//https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_1_5.shtml
					return StatusCode(400, returnData);
				}

				#region 记录日志（也可以记录到数据库审计日志中）
				var logDir = ServerUtility.ContentRootMapPath(string.Format("~/App_Data/TenPayNotify/{0}", SystemTime.Now.ToString("yyyyMMdd")));
				if (!Directory.Exists(logDir)) {
					Directory.CreateDirectory(logDir);
				}

				var logPath = Path.Combine(logDir, string.Format("{0}-{1}-{2}.txt", SystemTime.Now.ToString("yyyyMMdd"), SystemTime.Now.ToString("HHmmss"), Guid.NewGuid().ToString("n").Substring(0, 8)));

				using (var fileStream = System.IO.File.OpenWrite(logPath)) {
					var notifyJson = orderReturnJson.ToString();
					await fileStream.WriteAsync(Encoding.Default.GetBytes(notifyJson), 0, Encoding.Default.GetByteCount(notifyJson));
					fileStream.Close();

				}
				#endregion

				// 成功处理回调消息
				return StatusCode(200);
			} catch (Exception ex) {
				_logger.LogError(ex, "支付回调");
				WeixinTrace.WeixinExceptionLog(new WeixinException(ex.Message, ex));

				returnData.code = "FAIL";
				returnData.message = "服用器错误";
				return StatusCode(500, returnData);
				throw;
			}
		}
		#endregion
	}
}
