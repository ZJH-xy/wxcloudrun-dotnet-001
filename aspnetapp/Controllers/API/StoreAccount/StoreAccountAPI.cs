using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using static aspnetapp.Models.Order;
using Microsoft.CodeAnalysis;
using aspnetapp.Controllers.Miniprogram;
using aspnetapp.Controllers.API.Miniprogram;
using aspnetapp.Models;
using Senparc.Weixin.TenPayV3.Apis.BasePay;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.TenPayV3.Apis;
using Polly.Caching;
using Senparc.CO2NET.Utilities;
using Senparc.Weixin.TenPayV3;
using Senparc.CO2NET.Extensions;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace aspnetapp.Controllers.API.StoreAccount
{

    [Route("storeAccount")]
    [ApiController]
    [Authorize(Roles = "store")]// 只有商家能访问
    public class StoreAccountAPI : ControllerBase {
        private readonly MyDbContext _dbContext;
        private readonly IOptionsSnapshot<JWTSettings> _JWTSettingsOpt;
        private readonly ILogger<StoreAccountAPI> _logger;
        private readonly StoreAccountController storeAccountController;

        public StoreAccountAPI(MyDbContext dbContext, IOptionsSnapshot<JWTSettings> jWTSettingsOpt, ILogger<StoreAccountAPI> logger) {
            _dbContext = dbContext;
            _JWTSettingsOpt = jWTSettingsOpt;
            _logger = logger;
            storeAccountController = new(_dbContext);
        }

        /// <summary>
        /// 商家登录
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [AllowAnonymous]// 允许匿名访问
        [HttpGet("login/{account}/{password}")]
        public async Task<IActionResult> Login(string account, string password) {
            if (password is null)
                return StatusCode(403, "密码为空");

            /* 商家帐号判断 */
            Models.StoreAccount? storeAccount;
            try {
                storeAccount = await _dbContext.StoreAccount.SingleOrDefaultAsync(sa => sa.Account == account && sa.Password == password);

            } catch (Exception e) {
                _logger.LogError(e, "查询商家帐号");
                return StatusCode(500);
			}

            if (storeAccount is null)
                return StatusCode(403, "帐号或密码错误");

            /* 门店判断 */
            if (await _dbContext.Store.AnyAsync(s => s.Id == storeAccount.Id && s.IsDelete))// 门店是否删除
                return StatusCode(403, "门店不存在");

            return StatusCode(200, new { JWT = GetJwtToken(CreateClaim(storeAccount.Id.ToString(), "store")), TheStore = storeAccount.TheStore });
        }

        /// <summary>
        /// 商家获取订单信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("confirm/order/{orderId}")]
        public async Task<IActionResult> GetConfirmOder(int orderId) {
            Order? order;
            try {
                order = await storeAccountController.GetConfirmOder(orderId);

            } catch (Exception e) {
                _logger.LogError(e, "查询订单{OrderId}", orderId);
                return StatusCode(500);
			}

            if (order is null)
                return StatusCode(404);// 订单不存在

            if (order.Status != Order.EOrderStatus.待确认)
                return StatusCode(403, "订单状态异常");

            return StatusCode(200, new ReturnConfirmOrder(order));
        }

        #region 商家确认订单
        /// <summary>
        /// 商家确认订单
        /// </summary>
        /// <param name="getData"></param>
        /// <returns></returns>
        [HttpPost("confirm/order")]
        public async Task<IActionResult> ConfirmAnOrder(GetConfirmOrder getData) {
            /* 检查订单状态 */
            Order? order;
            try {
                order = await _dbContext.Order.FindAsync(getData.OderId);

            } catch (Exception e) {
                _logger.LogError(e, "获取订单{OderId}", getData.OderId);
                return StatusCode(500);
			}

            if (order is null)
                return StatusCode(403, "订单不存在");

            if (order.Status != Order.EOrderStatus.待确认)
                return StatusCode(403, "订单状态异常");

            /* 检查车辆状态 */
            Vehicle? vehicle;
            try {
                vehicle = await _dbContext.Vehicle.FindAsync(getData.TheVehicle);

            } catch (Exception e) {
                _logger.LogError(e, "查询车辆{VehicleId}", getData.TheVehicle);
                return StatusCode(500);
			}

            if (vehicle is null)
                return StatusCode(403, "车辆不存在");

            // 检查车辆是否属于当前商家
            int storeId;
            try {
                storeId = await _dbContext.StoreAccount.Where(sa => sa.Id == GetUserIdInt()).Select(sa => sa.TheStore).FirstAsync();

            } catch (Exception e) {
                _logger.LogError(e, "获取商家{VehicleId}的门店Id", GetUserIdInt());
                return StatusCode(500);
			}
            if (vehicle.TheCurrentStore != storeId)// 车辆当前所在门店
                return StatusCode(403, "车辆不在当前门店");

			// 车辆状态不为锁定或空闲
			if (vehicle.State is not Vehicle.Estates.锁定 and not Vehicle.Estates.空闲)
                return StatusCode(403, "车辆状态异常");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();// 事务开始

            DateTime now = DateTime.Now;

            vehicle.State = Vehicle.Estates.已出租;
            try {/* 确认订单 */
                order.Status = Order.EOrderStatus.进行中;
                order.ActualStartingTime = now;
				await _dbContext.SaveChangesAsync();

				// 商家选择不同车辆
				if (order.TheVehicle != getData.TheVehicle) {
                    Vehicle oldV = await _dbContext.Vehicle.SingleAsync(v => v.Id == order.TheVehicle);
                    oldV.State = Vehicle.Estates.空闲;
                    oldV.UpdatedAt = now;
                    oldV.StateUpdatedAt = now;
                    _dbContext.Update(oldV);
                    await _dbContext.SaveChangesAsync();

                    order.TheVehicle = getData.TheVehicle;
                    order.UpdatedAt = now;
                    vehicle.State = Vehicle.Estates.已出租;
                    vehicle.UpdatedAt = now;
                    vehicle.StateUpdatedAt = now;
                    _dbContext.Update(vehicle);
                    _dbContext.Update(order);
                    await _dbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();// 提交事务

            } catch (Exception e) {
                _logger.LogError(e, "确认订单事务失败，订单{OrderId}，车辆{VehicleId}状态更改", order.Id, vehicle.Id);
                await transaction.RollbackAsync();// 回滚事务
                return StatusCode(500);
			}

            return StatusCode(200);
        }
        #endregion

        /// <summary>
        /// 商家获取换车信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("confirm/vehicle/replacement/{orderId}")]
        public async Task<IActionResult> GetConfirmVehicleReplacement(int orderId) {
            VehicleReplacementRecord? vrr;

            try {
                vrr = await _dbContext.VehicleReplacementRecord.FirstOrDefaultAsync(v => v.TheOrder == orderId);// 获取换车信息

            } catch (Exception e) {
                _logger.LogError(e, "查询换车请求{OrderId}", orderId);
                return StatusCode(500);
            }

            if (vrr is null)
                return StatusCode(403, "请求不存在");

            // 获取订单状态
            Order order;
            try {
                order = await _dbContext.Order.FirstAsync(o => o.Id == orderId);

            } catch (Exception e) {
                _logger.LogError(e, "获取订单{OrderId}", orderId);
                return StatusCode(500);
				
			}

            // 获取套餐
            StoreMenu storeMenu;
            try {
                storeMenu = await _dbContext.StoreMenus.FirstAsync(sm => sm.Id == order.TheStoreMenu);

            } catch (Exception e) {
                _logger.LogError(e, "获取套餐{StoreMenuId}", order.TheStoreMenu);
                return StatusCode(500);
				
			}

            return StatusCode(200, new Returnreplacement(vrr, order, storeMenu));
        }

        /// <summary>
        /// 商家确认换车
        /// </summary>
        /// <returns></returns>
        [HttpPost("confirm/vehicle/replacement")]
        public async Task<IActionResult> ConfirmVehicleReplacement(GetConfirmReplacement getData) {
            /* 检查订单状态 */
            Order? order;
            try {
                order = await storeAccountController.GetConfirmOder(getData.OderId);

            } catch (Exception e) {
                _logger.LogError(e, "订单{OrderId}查询", getData.OderId);
                return StatusCode(500);
				
			}

            if (order is null)
                return StatusCode(403, "订单不存在");

            if (order.Status != Order.EOrderStatus.进行中)
                return StatusCode(403, "订单状态异常");

            /* 检查换车记录 */
            VehicleReplacementRecord? vrr;
            try {
                // 查询当前订单的换车请求
                vrr = await _dbContext.VehicleReplacementRecord.FirstAsync(v => v.TheOrder == getData.OderId && v.State == VehicleReplacementRecord.Estates.侍确认);

            } catch (Exception e) {
                _logger.LogError(e, "换车记录表OrderId=={OrderId}查询", getData.OderId);
                return StatusCode(500);
				
			}

            if (vrr is null)
                return StatusCode(403, "换车请求不存在");

            //if (vrr.State != VehicleReplacementRecord.Estates.侍确认)
            //    return StatusCode(403, "请求状态异常");

            /* 检查车辆状态 */
            Vehicle? newVehicle;
            try {
                // 查询将要更换的车辆
                newVehicle = await _dbContext.Vehicle.FindAsync(getData.TheVehicle);

            } catch (Exception e) {
                _logger.LogError(e, "查询车辆{VehicleId}", getData.TheVehicle);
                return StatusCode(500);
				
			}

            if (newVehicle is null)
                return StatusCode(403, "车辆不存在");

            // 检查车辆是否属于当前商家
            int storeId;
            try {
                storeId = await _dbContext.StoreAccount.Where(sa => sa.Id == GetUserIdInt()).Select(sa => sa.TheStore).FirstAsync();

            } catch (Exception e) {
                _logger.LogError(e, "根据商家帐号Id{VehicleReplacementRecordId}获取门店", GetUserIdInt());
                return StatusCode(500);
			}
            if (newVehicle.TheCurrentStore != storeId)// 车辆当前所在门店
                return StatusCode(403, "车辆不在当前门店");

            //if (newVehicle.State != Vehicle.Estates.锁定)
            //    return StatusCode(403, "车辆状态异常");

            // 获取用户将要更换的旧车辆
            Vehicle? oldVehicle;
            try {
                oldVehicle = await _dbContext.Vehicle.FirstAsync(v => v.Id == order.TheVehicle);

            } catch (Exception e) {
                _logger.LogError(e, "获取车辆{VehicleId}", GetUserIdInt());
                return StatusCode(500);
				
			}

            using var transaction = await _dbContext.Database.BeginTransactionAsync();// 事务开始

            DateTime now = DateTime.Now;
            /* 更改租用车辆 */
            order.TheVehicle = newVehicle.Id;// 更改订单租用车辆
            order.UpdatedAt = now;

            // 新车辆状态改为已出租
            newVehicle.State = Vehicle.Estates.已出租;

            /* 车辆状态改为侍确认 */
            oldVehicle.State = Vehicle.Estates.侍确认;
            oldVehicle.UpdatedAt = now;

            /* 换车记录表VehicleReplacementRecord 更新 */
            vrr.State = VehicleReplacementRecord.Estates.已完成;
            vrr.TheStore = GetUserIdInt();
            vrr.UpdatedAt = now;

            try {
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();// 提交事务

            } catch (Exception e) {
                _logger.LogError(e, "更换车辆事务失败，订单表{OrderId}，换车记录表{换车记录表VehicleReplacementRecordId}", order.Id, vrr.Id);
                await transaction.RollbackAsync();// 回滚事务
                return StatusCode(500);
			}

            return StatusCode(200);
        }

        /// <summary>
        /// 商家获取还车信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("confirm/vehicle/return/{orderId}")]
        public async Task<IActionResult> GetConfirmVehicleReturn(int orderId) {
            Order? order = await storeAccountController.GetConfirmOder(orderId);

            if (order is null)
                return StatusCode(404, "订单不存在");// 订单不存在

            if (order.Status != Order.EOrderStatus.进行中)
                return StatusCode(403, "订单状态异常");

            return StatusCode(200, new ReturnOrder(order));
        }

        #region 商家确认还车
        /// <summary>
        /// 商家确认还车
        /// </summary>
        /// <returns></returns>
        [HttpPost("confirm/vehicle/return")]
        public async Task<IActionResult> ConfirmVehicleReturn(GetConfirmReturn getData) {
            /* 检查订单状态 */
            Order? order;
            try {
                order = await storeAccountController.GetConfirmOder(getData.OderId);

            } catch (Exception e) {
                _logger.LogError(e, "订单{OrderId}查询", getData.OderId);
                return StatusCode(500);
			}

            if (order is null)
                return StatusCode(403, "订单不存在");

            if (order.Status != Order.EOrderStatus.进行中)
                return StatusCode(403, "订单状态异常");

            // 检查是否有未完成的换车请求
            if (await _dbContext.VehicleReplacementRecord.AnyAsync(vrr => vrr.TheOrder == getData.OderId && vrr.State == VehicleReplacementRecord.Estates.侍确认))
                return StatusCode(403, "当前有未完成的换车请求");

            // 获取套餐时间
            StoreMenu? storeMenu;
            try {
                storeMenu = await _dbContext.StoreMenus.FirstAsync(sm => sm.Id == order.TheStoreMenu);

            } catch (Exception e) {
                _logger.LogError(e, "套餐{StoreMenuId}查询", order.TheStoreMenu);
                return StatusCode(500);
			}

            using var transaction = await _dbContext.Database.BeginTransactionAsync();/* 事务开始 */

            /* 判断超时 */
            // 当前时间
            var now = DateTime.Now;

            // 超时费率 (每小时5元)
            decimal overtimeRate = 5;

            // 订单开始时间
            DateTime actualStartingTime = (DateTime)order.ActualStartingTime!;

            // 免费时间长度 (1小时)
            TimeSpan freeDuration = TimeSpan.FromHours(1);

            // 计算订单已经持续的时间
            TimeSpan elapsedTime = now - actualStartingTime;

            // 如果超时，计算超时费用
            if (elapsedTime > freeDuration) {
                // 超出免费时间的部分
                TimeSpan overtime = elapsedTime - freeDuration;

                // 按小时计算超时费用（向上取整到整小时）
                int overtimeHours = (int)Math.Ceiling(overtime.TotalHours);

                // 计算总超时费用
                order.OvertimeFee += overtimeHours * overtimeRate;
            }

            // 调度
            if (order.TheRentalLocation != GetUserIdInt()) {
                order.DispatchFee += 10;
            }

            order.ActualReturnTime = now;// 更新订单归还时间
            DateTime startingTime = (DateTime)order.ActualStartingTime!;// 租车开始时间
            DateTime expireTime = startingTime.AddHours(storeMenu.Duration);// 开始时间加上套餐时间

            // 超时60 分钟
            //if (now >= expireTime.AddMinutes(60)) {
            //    // 更新超时费
            //    TimeSpan overtime = now - expireTime;
            //    order.OvertimeFee = (decimal)(overtime.Hours * 5);// 超时费，一小时5 元
            //}

            // 实际费用
            var totalPrice = order.GetActualCost();

			// 已付金额小于总金额
			if (order.Paid < order.GetTotalPrice()) {
                // 要求用户支付剩余金额
                order.Status = EOrderStatus.侍补余;
                try {
                    _dbContext.Order.Update(order);
                    await _dbContext.SaveChangesAsync();

                } catch (Exception e) {
                    _logger.LogError(e, "[订单结算]更改订单{OrderId}状态为侍补余", order.Id);
                }

                // 创建补充订单
                SupplementaryOrders supplementaryOrders = new() {
                    TheOrder = order.Id,
                    OutTradeNo = string.Concat("SRental_", Guid.NewGuid().ToString("N").AsSpan(0, 20)),
                    //TransactionId = "",//等
                    Total = Order.GetTotal(order.GetTotalPrice() - order.Paid),
                    Status = EOrderStatus.待付款,
                    //SuccessTime
                    CreatedAt = now,
                    UpdatedAt = now
                };

                try {
                    await _dbContext.SupplementaryOrders.AddAsync(supplementaryOrders);
                    await _dbContext.SaveChangesAsync();

                } catch (Exception e) {
                    _logger.LogError(e, "创建补充订单{supplementaryOrders}失败", supplementaryOrders.ToJson(true));
                    return StatusCode(500);
                }

                return StatusCode(200);
                //return StatusCode(202, new { appid, timestamp, nonceStr, pack, signType, paySign });
            } else if (order.Paid > totalPrice) {
                // 进入退款
                // 退押金金额（金额修改完成后计算！！！）

                // 新建退款表数据
                RefundOrder refundOrder = new() {
                    TheOrder = order.Id,
					//outRefundNo = string.Concat("Refund_", Guid.NewGuid().ToString("N").AsSpan(0, 20)),
					RefundId = "",//等待
                    Reason = "自动退款",
                    Status = RefundOrder.Estatus.已创建,//等待
                    Total = order.Paid,
                    Refund = totalPrice,//归还多余费用
                    SuccessTime = null,//等待
                    CreateTime = now,//等待
                    UpdatedAt = now,
                };

                try {
					// 调用退款服务
					RefundReturnJson refundReturnJson = await OrderAPI.RefundAsync(order, refundOrder);

					if (refundReturnJson.ResultCode.Success != true) {
						_logger.LogError("退款请求失败{ResultCode}", refundReturnJson.ResultCode.ToJson(true));
						return StatusCode(403, "退款请求失败，请稍后再试");
					}

					// 生成退款表数据
					_dbContext.RefundOrder.Add(refundOrder);
					await _dbContext.SaveChangesAsync();

					order.Status = Order.EOrderStatus.退款中;
                    order.ActualReturnTime = now;// 归还时间
                    order.TheReturnThePoint = GetUserIdInt();
                    order.DepositRefunded += refundOrder.Refund;
                    order.UpdatedAt = now;
					await _dbContext.SaveChangesAsync();

					Vehicle vehicle = await _dbContext.Vehicle.SingleAsync(v => v.Id == order.TheVehicle);
					vehicle.State = Vehicle.Estates.侍确认;
					vehicle.StateUpdatedAt = now;
					await _dbContext.SaveChangesAsync();

				} catch (Exception ex) {
                    _logger.LogError(ex, "订单处理失败。订单ID: {OrderId}, 车辆ID: {VehicleId}, 退款金额: {RefundAmount}", order.Id, order.TheVehicle, refundOrder.Refund);
                    return StatusCode(500);
                }
            } else {
                try {
					// 金额相等
					Vehicle vehicle = await _dbContext.Vehicle.SingleAsync(v => v.Id == order.TheVehicle);
					vehicle.State = Vehicle.Estates.侍确认;
					vehicle.StateUpdatedAt = now;
					await _dbContext.SaveChangesAsync();

					order.Status = Order.EOrderStatus.已完成;
					order.ActualReturnTime = now;// 归还时间
					order.TheReturnThePoint = GetUserIdInt();
					order.UpdatedAt = now;
					await _dbContext.SaveChangesAsync();

				} catch (Exception e) {
                    _logger.LogError(e, "金额相等无退款订单处理失败。订单ID: {OrderId}, 车辆ID: {VehicleId}", order.Id, order.TheVehicle);

					return StatusCode(500);
				}
			}

            // 营业额统计表
            RevenueStatistics revenueStatistics = new() {
                TheStoreA = order.TheRentalLocation,
                TheStoreB = GetUserIdInt(),
                TheOrder = order.Id,
                CreatedAt = now,
                UpdatedAt = now,
            };

            try {
                await _dbContext.RevenueStatistic.AddAsync(revenueStatistics);
                await _dbContext.SaveChangesAsync();

            } catch (Exception ex) {
                _logger.LogError(ex, "营业额统计表添加失败{data}", revenueStatistics.ToJson(true));
            }

            // 最后操作
            try {
                await transaction.CommitAsync();

            } catch (Exception e) {
                _logger.LogCritical(e, "确认还车事务提交失败orderId{Id},TheVehicle:{TheVehicle},TheStoreId:{TheStore}", order.Id, order.TheVehicle, GetUserIdInt());
                await transaction.RollbackAsync();

                return StatusCode(500);
            }

            return StatusCode(200);
        }
		#endregion

		#region 商家提交地址审核
		/// <summary>
		/// 商家提交地址审核
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost("reviewMerchantAddress")]
        public async Task<IActionResult> PostReviewMerchantAddress(GetAddress data) {
            var reviewMerchantAddress = new ReviewMerchantAddress() {
                TheStore = GetUserIdInt(),
				Name = data.Name,
				Address = data.Address,
				GpsLongitude = data.GpsLongitude,
                GpsLatitude = data.GpsLatitude,
            };

            try {
                await _dbContext.ReviewMerchantAddress.AddAsync(reviewMerchantAddress);
                await _dbContext.SaveChangesAsync();

            } catch (Exception e) {
                _logger.LogError(e, "保存商家提交地址审核失败");
                return StatusCode(500);
            }

            return Ok();
        }
        #endregion

        #region 接收支付回调
        /// <summary>
        /// 支付回调（侍测试）
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]// 允许匿名访问
        [HttpPost("callback/notify")]
        public async Task<IActionResult> SupplementaryOrders() {
            _logger.LogInformation("SupplementaryOrders收到微信支付回调");

            WxPayCallbackViewModel returnData = new();// 创建应答格式
            try {
                //获取微信服务器异步发送的支付通知信息
                TenPayNotifyHandler resHandler = new TenPayNotifyHandler(HttpContext);
                OrderReturnJson orderReturnJson = await resHandler.DecryptGetObjectAsync<OrderReturnJson>();

                //记录日志
                _logger.LogInformation("SupplementaryOrders收到微信支付回调：{data}", orderReturnJson.ToJson(true));
                Senparc.Weixin.WeixinTrace.SendCustomLog("SupplementaryOrders 接收到消息", orderReturnJson.ToJson(true));

                //演示记录 transaction_id，实际开发中需要记录到数据库，以便退款和后续跟踪
                // transaction_id 微信支付系统生成的订单号。
                //Order? order = await _orderController.GetById(GetUserIdInt(), int.Parse(orderReturnJson.out_trade_no));// 根据Id获取对应的订单
                SupplementaryOrders supplementaryOrders = await _dbContext.SupplementaryOrders.SingleAsync(o => o.OutTradeNo == orderReturnJson.out_trade_no);

                if (supplementaryOrders is null) {
                    _logger.LogError("订单获取错误transaction_id：{transaction_id}", orderReturnJson.out_trade_no);
                    throw new Exception("订单获取错误transaction_id");
                }

                //获取支付状态
                string trade_state = orderReturnJson.trade_state;

                //验证请求是否从微信发过来（安全）

                //验证可靠的支付状态
                if (orderReturnJson.VerifySignSuccess == true) {
                    var now = DateTime.Now;
                    supplementaryOrders.TransactionId = orderReturnJson.transaction_id;// 赋值微信传入的id
                    supplementaryOrders.UpdatedAt = now;

                    try {
                        await _dbContext.SaveChangesAsync();
                    } catch (Exception e) {
                        _logger.LogError(e, "赋值微信传入的id");
                        throw;
                    }

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
                            if (supplementaryOrders.Status == Order.EOrderStatus.已完成) {
                                return StatusCode(200);
                            }

                            // 微信支付订单号查询订单，二次验证
                            BasePayApis basePayApis = new();
                            string mchid = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId;
                            var trade = await basePayApis.OrderQueryByTransactionIdAsync(new QueryRequestData(mchid, supplementaryOrders.TransactionId));

                            if (trade.trade_state != "SUCCESS") {
                                returnData.code = "FAIL";//错误的订单处理
                                returnData.message = "订单状态不一致";
                                return StatusCode(500, returnData);
                            }

                            /* 提示：
							* 1、直到这里，才能认为交易真正成功了，可以进行数据库操作，但是别忘了返回规定格式的消息！
							* 2、上述判断已经具有比较高的安全性以外，还可以对访问 IP 进行判断进一步加强安全性。
							* 3、下面演示的是发送支付成功的模板消息提示，非必须。
							*/
                            using (var transaction = await _dbContext.Database.BeginTransactionAsync()) {
                                try {
                                    supplementaryOrders.Status = Order.EOrderStatus.已完成;// 更改订单状态
                                    supplementaryOrders.Paid += orderReturnJson.amount.total / 100m;// 增加已付金额,在代码中将 `total` 转换为元
                                    supplementaryOrders.SuccessTime = orderReturnJson.success_time;
                                    supplementaryOrders.UpdatedAt = now;
                                    await _dbContext.SaveChangesAsync();

                                    Order order = await _dbContext.Order.SingleAsync(o => o.Id == supplementaryOrders.TheOrder);
                                    order.Status = EOrderStatus.已补余;
                                    //order.Paid += orderReturnJson.amount.total;
                                    order.UpdatedAt = now;
                                    await _dbContext.SaveChangesAsync();

                                    await transaction.CommitAsync();
                                    _logger.LogInformation("更改完毕");
                                } catch (Exception e) {
                                    _logger.LogCritical(e, "支付回调{orderReturnJson}", orderReturnJson.ToJson(true));
                                    await transaction.RollbackAsync();

                                    returnData.code = "FAIL";//错误的订单处理
                                    returnData.message = "服务器错误";
                                    return StatusCode(500, returnData);
                                }
                            }
                            break;

                        case "CLOSED":// 已关闭
                            using (var transaction = await _dbContext.Database.BeginTransactionAsync()) {
                                try {
                                    _logger.LogInformation("订单已关闭");
                                    supplementaryOrders.Status = Order.EOrderStatus.已取消;
                                    supplementaryOrders.UpdatedAt = now;
                                    await _dbContext.SaveChangesAsync();

                                    await transaction.CommitAsync();
                                    _logger.LogInformation("处理完毕");

                                } catch (Exception e) {
                                    _logger.LogError(e, "支付回调{orderReturnJson}", orderReturnJson.ToJson(true));
                                    await transaction.RollbackAsync();

                                    returnData.code = "FAIL";//错误的订单处理
                                    returnData.message = "服务器错误";
                                    return StatusCode(500, returnData);
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
                    _logger.LogInformation("回调验证失败");
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

            }
        }
        #endregion

        /// <summary>
        /// JWT 获取用户id
        /// </summary>
        /// <returns></returns>
        public int GetUserIdInt() {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        /// <summary>
        /// 创建Claim
        /// </summary>
        /// <param name="id"></param>
        /// <param name="role">角色</param>
        /// <returns>List<Claim> 对象</returns>
        public List<Claim> CreateClaim(string id, string role) {
            List<Claim> claims = new() {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Role, role)
            };
            return claims;
        }

        /// <summary>
        /// JWT 令牌计算
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public string GetJwtToken(List<Claim> claims) {
            // 读取配置
            string key = _JWTSettingsOpt.Value.SecKey;
            DateTime expires = DateTime.Now.AddDays(_JWTSettingsOpt.Value.ExpireDays);// 读取配置过期时间

            // 计算
            byte[] secBytes = Encoding.UTF8.GetBytes(key);
            var secKey = new SymmetricSecurityKey(secBytes);
            var credentials = new SigningCredentials(secKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new JwtSecurityToken(claims: claims,
                expires: expires, signingCredentials: credentials);
            string jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

            _logger.LogDebug("角色：{Role}，ID：{NameIdentifier}生成新JWTtoken：{claims}",
                claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value,
                claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                claims.ToJToken());
            return jwt;
        }
    }

    /// <summary>
    /// 商家审核地址信息
    /// </summary>
    public class GetAddress {
        /// <summary>
        /// Longitude 经度，范围 [-180, 180]
        /// </summary>
        public double GpsLongitude { get; set; }

        /// <summary>
        /// Latitude 纬度，范围 [-90, 90]
        /// </summary>
        public double GpsLatitude { get; set; }
		public string Name { get; set; }
		public string Address { get; set; }
	}

    /// <summary>
    /// 确认订单格式
    /// </summary>
    public class GetConfirmOrder {
        public int OderId { get; set; }
        public int TheVehicle { get; set; }// 租用车辆
    }

    /// <summary>
    /// 确认换车格式
    /// </summary>
    public class GetConfirmReplacement {
        public int OderId { get; set; }
        public int TheVehicle { get; set; }// 更换车辆
    }

    /// <summary>
    /// 确认还车格式
    /// </summary>
    public class GetConfirmReturn {
        public int OderId { get; set; }
    }

    /// <summary>
    /// 商家获取确认订单返回格式
    /// </summary>
    public struct ReturnConfirmOrder {
        public ReturnConfirmOrder(Order order) {
            Id = order.Id;
            TheVehicle = order.TheVehicle;
            TheStoreMenu = order.TheStoreMenu;
            TheRentalLocation = order.TheRentalLocation;
            UserName = order.UserName;
            UserPhone = order.UserPhone;
			IdentityCard = order.IdentityCard;
			Deposit = order.Deposit;
            Rent = order.Rent;
            Paid = order.Paid;
            Status = order.Status;
            CreatedAt = order.CreatedAt;
            Notes = order.Notes;
        }
        public int Id { get; init; }// 订单编号
        public int TheVehicle { get; set; }// 租用车辆
        public int TheStoreMenu { get; set; }// 套餐
        public int TheRentalLocation { get; set; }// 租车点（StoreId）
        public string UserName { get; set; }// 用户姓名
        public string UserPhone { get; set; }// 用户手机号
		public string? IdentityCard { get; set; }// 身份证号
		public decimal Deposit { get; set; }// 押金
        public decimal Rent { get; set; }// 租金
        public decimal Paid { get; set; }// 已付
        public Order.EOrderStatus Status { get; set; }// 订单状态
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }// 备注
        //public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// 商家获取确认换车返回格式
    /// </summary>
    public struct Returnreplacement {
        public Returnreplacement(VehicleReplacementRecord vrr, Order order, StoreMenu storeMenu) {
            TheOldVehicles = vrr.TheOldVehicles;
            TheNewVehicles = vrr.TheNewVehicles;
            CreatedAt = vrr.CreatedAt;
            TheOrder = order.Id;
            TheRentalLocation = order.TheRentalLocation;
            UserName = order.UserName;
            UserPhone = order.UserPhone;
            IdentityCard = order.IdentityCard;
            Notes = order.Notes;
            Duration = storeMenu.Duration;
        }
        public int TheOldVehicles { get; set; }// 旧车辆
        public int TheNewVehicles { get; set; }// 新车辆
        public DateTime CreatedAt { get; set; }
        // 订单相关
        public int TheOrder { get; set; }// 租车点（StoreId）
        public int TheRentalLocation { get; set; }// 租车点（StoreId）
        public string UserName { get; set; }// 用户姓名
        public string UserPhone { get; set; }// 用户手机号
        public string? IdentityCard { get; set; }// 身份证号
        public string? Notes { get; set; }// 备注
        // 套餐相关
        public int Duration { get; set; }// 小时时长
    }

    /// <summary>
    /// 商家确认还车返回格式
    /// </summary>
    public struct ReturnOrder {
        public ReturnOrder(Order order) {
            Id = order.Id;
            TheVehicle = order.TheVehicle;
            TheStoreMenu = order.TheStoreMenu;
            ActualStartingTime = order.ActualStartingTime;
            TheRentalLocation = order.TheRentalLocation;
            UserName = order.UserName;
            UserPhone = order.UserPhone;
            IdentityCard = order.IdentityCard;
			Deposit = order.Deposit;
            Rent = order.Rent;
            Paid = order.Paid;
            Status = order.Status;
            CreatedAt = order.CreatedAt;
            Notes = order.Notes;
        }
        public int Id { get; init; }// 订单编号
        public int TheVehicle { get; set; }// 租用车辆
        public DateTime? ActualStartingTime { get; set; }// 实际起始时间
        public int TheStoreMenu { get; set; }// 套餐
        public int TheRentalLocation { get; set; }// 租车点（StoreId）
        public string UserName { get; set; }// 用户姓名
        public string UserPhone { get; set; }// 用户手机号
		public string? IdentityCard { get; set; }// 身份证号
        public decimal Deposit { get; set; }// 押金
        public decimal Rent { get; set; }// 租金
        public decimal Paid { get; set; }// 已付
        public Order.EOrderStatus Status { get; set; }// 订单状态
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }// 备注
        //public DateTime UpdatedAt { get; set; }
    }
}
