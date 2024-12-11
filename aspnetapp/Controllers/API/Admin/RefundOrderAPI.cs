using aspnetapp.Controllers.API.Miniprogram;
using Microsoft.EntityFrameworkCore;
using Senparc.Weixin.TenPayV3.Apis.BasePay;

namespace aspnetapp.Controllers.API.Admin {

	[Route("admin")]
	[ApiController]
	public class RefundOrderAPI : ControllerBase {
		private readonly MyDbContext _dbContext;
		private readonly IOptionsSnapshot<JWTSettings> _JWTSettingsOpt;
		private readonly ILogger<RefundOrderAPI> _logger;

		public RefundOrderAPI(MyDbContext dbContext, IOptionsSnapshot<JWTSettings> jWTSettingsOpt, ILogger<RefundOrderAPI> logger) {
			_dbContext = dbContext;
			_JWTSettingsOpt = jWTSettingsOpt;
			_logger = logger;
		}

		/// <summary>
		/// 自选金额退款
		/// </summary>
		/// <param name="getData"></param>
		/// <returns></returns>
		[HttpPost("RefundOrderAll")]
		public async Task<IActionResult> RefundOrderAll(GetData1 getData) {
			Order? order;
			try {
				order = await _dbContext.Order.SingleOrDefaultAsync(o => o.Id == getData.TheOrder);

			} catch (Exception e) {
				return StatusCode(500, e);
			}

			if (order == null)
				return StatusCode(404, "未找到订单");

			var now = DateTime.Now;

			// 新建退款表数据
			RefundOrder refundOrder = new() {
				TheOrder = order.Id,
				//outRefundNo = string.Concat("Refund_", Guid.NewGuid().ToString("N").AsSpan(0, 20)),
				RefundId = "",//等待
				Reason = "管理员退款",
				Status = RefundOrder.Estatus.已创建,//等待
				Total = order.Paid,
				Refund = getData.Refund,//退还费用
				SuccessTime = null,//等待
				CreateTime = now,//等待
				UpdatedAt = now,
			};

			using var transaction = await _dbContext.Database.BeginTransactionAsync();// 事务开始

			try {
				// 调用退款服务
				RefundReturnJson refundReturnJson = await OrderAPI.RefundAsync(order, refundOrder);

				if (refundReturnJson.ResultCode.Success != true) {
					return StatusCode(403, refundReturnJson.ToJson());
				}

				order.Status = Order.EOrderStatus.退款中;

				// 生成退款表数据
				_dbContext.RefundOrder.Add(refundOrder);
				await _dbContext.SaveChangesAsync();

				// 如果退款成功，提交事务
				await transaction.CommitAsync();
			} catch (Exception e) {
				await transaction.RollbackAsync();

				return StatusCode(500, e);
			}
			return StatusCode(200);
		}

		/// <summary>
		/// 补余退款
		/// </summary>
		/// <param name="getData"></param>
		/// <returns></returns>
		[HttpPost("RefundSupplementaryOrders")]
		public async Task<IActionResult> RefundSupplementaryOrders(GetData1 getData) {
			SupplementaryOrders? supplementaryOrders;
			try {
				supplementaryOrders = await _dbContext.SupplementaryOrders.SingleOrDefaultAsync(o => o.Id == getData.TheOrder);

			} catch (Exception e) {
				return StatusCode(500, e);
			}

			if (supplementaryOrders == null)
				return StatusCode(404, "未找到订单");

			var now = DateTime.Now;

			// 新建退款表数据
			RefundOrder refundOrder = new() {
				TheOrder = supplementaryOrders.Id,
				//outRefundNo = string.Concat("Refund_", Guid.NewGuid().ToString("N").AsSpan(0, 20)),
				RefundId = "",//等待
				Reason = "管理员补余退款",
				Status = RefundOrder.Estatus.已创建,//等待
				Total = supplementaryOrders.Paid,
				Refund = getData.Refund,//退还费用
				SuccessTime = null,//等待
				CreateTime = now,//等待
				UpdatedAt = now,
			};

			using var transaction = await _dbContext.Database.BeginTransactionAsync();// 事务开始

			try {
				// 调用退款服务
				RefundReturnJson refundReturnJson = await OrderAPI.RefundAsync(new Order {
					TransactionId = supplementaryOrders.TransactionId,
					OutTradeNo = supplementaryOrders.OutTradeNo
				}, refundOrder);

				if (refundReturnJson.ResultCode.Success != true) {
					return StatusCode(403, refundReturnJson.ToJson());
				}

				// 生成退款表数据
				_dbContext.RefundOrder.Add(refundOrder);
				await _dbContext.SaveChangesAsync();

				// 如果退款成功，提交事务
				await transaction.CommitAsync();
			} catch (Exception e) {
				await transaction.RollbackAsync();

				return StatusCode(500, e);
			}
			return StatusCode(200);
		}
	}
}

public class GetData1 {
	/// <summary>
	/// 指向订单
	/// </summary>
	public int TheOrder { get; set; }
	/// <summary>
	/// 【退款金额】退款标价金额，单位为分，可以做部分退款
	/// </summary>
	public decimal Refund { get; set; }
}
