using aspnetapp.Controllers.Miniprogram;
using Microsoft.AspNetCore.Authorization;
using MySqlConnector;
using System.Security.Claims;
using Senparc.Weixin.TenPayV3.Apis;
using Senparc.Weixin.TenPayV3.Apis.BasePay;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.TenPayV3;
using Senparc.CO2NET.Utilities;
using Senparc.CO2NET.Extensions;
using Microsoft.CodeAnalysis;
using System.Collections;
using Senparc.Weixin.TenPayV3.Apis.BasePay.Entities;
using aspnetapp.Models;
using static aspnetapp.Models.Order;
using Org.BouncyCastle.Asn1.Cms;
using aspnetapp.Controllers.API.StoreAccount;

namespace aspnetapp.Controllers.API.Miniprogram {

	[Route("order")]
	[ApiController]
	[Authorize]// 方法受到限制
	public class OrderAPI : ControllerBase {
		private readonly MyDbContext _dbContext;
		private readonly ILogger<OrderAPI> _logger;
		private readonly OrderController _orderController;
		private readonly ConfigurationService _configService;

		public OrderAPI(MyDbContext dbContext, ILogger<OrderAPI> logger, ConfigurationService configService) {
			_dbContext = dbContext;
			_logger = logger;
			_orderController = new(_dbContext);
			_configService = configService;
		}

		/// <summary>
		/// 计算总租金
		/// </summary>
		/// <param name="rentalLocation">租车门店Id</param>
		/// <param name="menuId">套餐Id</param>
		/// <param name="depositRequired">需要押金</param>
		/// <returns></returns>
		/// 
		[AllowAnonymous]// 允许匿名访问
		[HttpGet("calculate/{rentalLocation}/{menuId}/{depositRequired}")]
		public async Task<IActionResult> CalculateRent(int rentalLocation, int menuId, bool depositRequired) {
			Store? store = await _dbContext.Store.SingleOrDefaultAsync(s => s.Id == rentalLocation && !s.IsDelete);
			if (store is null)
				return StatusCode(404);

			StoreMenu? storeMenus = await _dbContext.StoreMenus.SingleOrDefaultAsync(sm => sm.Id == menuId && sm.TheStore == store.Id && !sm.IsDelete);
			if (storeMenus is null)
				return StatusCode(404);

			decimal rent = storeMenus.Rent;
			decimal deposit = depositRequired ? storeMenus.Deposit : 0;

			return StatusCode(200, new { rent, deposit });
		}

		/// <summary>
		/// 用户查询单个订单
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		[HttpGet("i/{orderId}")]
		public async Task<IActionResult> GetOderById(int orderId) {
			Order? order;
			ReturnOrder returnOrder;

			try {
				order = await _orderController.GetById(GetUserIdInt(), orderId);

				if (order is null)
					return StatusCode(404);

				StoreMenu? storeMenu = await _dbContext.StoreMenus.FindAsync(order.TheStoreMenu);// 查找订单套餐

				returnOrder = new(order, storeMenu);

			} catch (Exception e) {
				_logger.LogError(e, "用户{UserId}查询订单{order}信息", GetUserIdInt(), orderId);

				return StatusCode(500);
			}

			if (order == null)
				return StatusCode(404);

			return StatusCode(200, returnOrder);
		}

		/// <summary>
		/// 获取用户最近10条订单
		/// </summary>
		/// <returns></returns>
		[HttpGet("a")]
		public async Task<IActionResult> GetOderByUserId() {
			List<Order> orderList = new();
			try {
				orderList = await _orderController.GetOrderByUserId(GetUserIdInt());

			} catch (Exception e) {
				_logger.LogError(e, "用户{UserId}查询订单信息", GetUserIdInt());

				return StatusCode(500);
			}

			foreach (var order in orderList) {
				// 如果在退款则请求
				if (order.Status == Order.EOrderStatus.退款中) {
					var refundOrder = await _dbContext.RefundOrder.SingleAsync(ro => ro.TheOrder == order.Id);// 退款数据

					BasePayApis basePayApis = new();
					RefundReturnJson refundReturnJson = await basePayApis.RefundQueryAsync(new RefundQueryRequestData(refundOrder.outRefundNo));// 查询退款信息

					if (refundReturnJson.ResultCode.Success != true) {
						_logger.LogError("请求获取退款信息失败{OrderId}", order.Id);
						continue;
					}

					if (refundReturnJson.status == "SUCCESS") {
						// 退款成功
						using var transaction = await _dbContext.Database.BeginTransactionAsync();// 事务

						order.Status = Order.EOrderStatus.已退款;
						order.UpdatedAt = DateTime.Now;

						refundOrder.SuccessTime = refundReturnJson.success_time;
						refundOrder.UpdatedAt = DateTime.Now;

						try {
							_dbContext.Order.Update(order);
							await _dbContext.SaveChangesAsync();
							await transaction.CommitAsync();

						} catch (Exception e) {
							_logger.LogError(e, "[GetOderByUserId]更新退款信息");
							await transaction.RollbackAsync();
							throw;
						}
					} else {
						// 其他状态
						refundOrder.Status = (RefundOrder.Estatus)RefundOrderEstatusHashtable[refundReturnJson.status]!;
						refundOrder.UpdatedAt = DateTime.Now;
						try {
							_dbContext.RefundOrder.Update(refundOrder);
							await _dbContext.SaveChangesAsync();

						} catch (Exception e) {
							_logger.LogError(e, "更新退款状态");
							throw;
						}
					}
				}
			}

			List<ReturnOrderBasic> returnOrderorderList = new();

			foreach (Order order in orderList)
				returnOrderorderList.Add(new ReturnOrderBasic(order));

			return StatusCode(200, returnOrderorderList);
		}

		/// <summary>
		/// 获取补余订单
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		[HttpGet("supplementaryOrders/{OrderId}")]
		public async Task<IActionResult> GetSupplementaryOrders(int orderId) {
			var supplementaryOrders = await _dbContext.SupplementaryOrders.Where(so => so.TheOrder == orderId).ToListAsync();

			if (supplementaryOrders is null) {
				return StatusCode(404);
			}

			List<ReturnSupplementaryOrder> rso = new();

			foreach (var ro in supplementaryOrders) {
				rso.Add(new ReturnSupplementaryOrder(ro));
			}

			return StatusCode(200, rso);
		}

		/// <summary>
		/// 获取订单状态
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		[HttpGet("status/i/{orderId}")]
		public async Task<IActionResult> GetOderStatusByUserId(int orderId) {
			Order.EOrderStatus? orderStatus;
			try {
				orderStatus = await _orderController.GetOrderStatusById(GetUserIdInt(), orderId);

			} catch (Exception e) {
				_logger.LogError(e, "用户{UserId}查询订单{order}状态", GetUserIdInt(), orderId);
				return StatusCode(500);
			}

			if (orderStatus == null)
				return StatusCode(404);

			return StatusCode(200, orderStatus);
		}

		/// <summary>
		/// 获取订单换车状态
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		[HttpGet("replacement/i/{orderId}")]
		public async Task<IActionResult> GetOderReplacementByUserId(int orderId) {
			bool status;
			try {
				status = await _orderController.GetOderReplacementByUserId(GetUserIdInt(), orderId);

			} catch (Exception e) {
				_logger.LogError(e, "用户{UserId}查询订单{order}换车状态", GetUserIdInt(), orderId);
				return StatusCode(500);

			}

			return StatusCode(200, status);
		}

		#region 创建订单
		/// <summary>
		/// 创建订单
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost("add")]
		public async Task<IActionResult> AddOrder(GetOrder data) {
			int userId = GetUserIdInt();
			// 订单信息合法性验证

			if (await _dbContext.User.SingleOrDefaultAsync(u => u.Id == userId) is null)
				return StatusCode(403, "用户不存在");

			// 需要押金为假，检查身份证格式
			if (!(data.DepositRequired || Judge.IdentityCardFormatDetermination(data.IdentityCard)))
				return StatusCode(403, "请检查身份证号格式");

			if (data.UserName == string.Empty)
				return StatusCode(403, "请检查名字格式");

			if (!Judge.PhoneFormatDetermination(data.UserPhone))
				return StatusCode(403, "请检查手机号码格式");

			// 检查用户订单状态
			try {
				if (await _dbContext.Order.Where(o => o.TheUser == userId).
					AnyAsync(o => o.Status == Order.EOrderStatus.待付款)) {
					return StatusCode(403, "当前有待付款的订单");
				}
			} catch (Exception e) {
				_logger.LogError(e, "查询用户{UserId}未完成的订单信息", userId);

				return StatusCode(404, "订单信息获取错误");
			}

			// 检查门店状态
			Store? store;
			StoreMenu? storeMenus = await _dbContext.StoreMenus.SingleOrDefaultAsync(sm => sm.Id == data.StoreMenuId && !sm.IsDelete);
			if (storeMenus == null)
				return StatusCode(403, "请检查套餐信息");

			store = await _dbContext.Store.SingleOrDefaultAsync(s => s.IsDelete == false && s.Id == storeMenus.TheStore);

			if (store == null)
				return StatusCode(404);

			if (!(store.BusinessStatus && store.IsOpen()))
				return StatusCode(403, "门店未营业");

			// 检查车辆状态
			Vehicle? vehicle;
			try {
				vehicle = await _dbContext.Vehicle.SingleOrDefaultAsync(v => v.IsDelete == false && v.Id == data.Vehicle);

			} catch (Exception e) {
				_logger.LogError(e, "获取车辆{VehicleId}信息", data.Vehicle);

				return StatusCode(505);
			}
			if (vehicle is null)
				return StatusCode(404, "车辆不存在或状态异常");

			if (vehicle.State != Vehicle.Estates.空闲)
				return StatusCode(403, "手慢了，请更换车辆");

			using var transaction = await _dbContext.Database.BeginTransactionAsync();// 事务开始

			try {// 锁定车辆，更新时间
				vehicle.State = Vehicle.Estates.锁定;
				vehicle.StateUpdatedAt = DateTime.Now;
				vehicle.UpdatedAt = DateTime.Now;

				await _dbContext.SaveChangesAsync();// 保存车辆状态
													//try {

				//} catch (Exception e) {
				//    _logger.LogError(e, "车辆{Vehicle}锁定，操作用户{UserId}", vehicle.Id, userId);
				//    return StatusCode(500, "车辆锁定失败");
				//}

				Order order = new() {
					TheUser = userId,
					TheRentalLocation = store.Id,
					OutTradeNo = string.Concat("Rental_", Guid.NewGuid().ToString("N").AsSpan(0, 20)),// 创建订单号
					TheVehicle = data.Vehicle,// 车辆
					TheStoreMenu = data.StoreMenuId,// 套餐Id
					UserName = data.UserName,// 用户姓名
					UserPhone = data.UserPhone,// 用户手机号
					IdentityCard = data.IdentityCard,// 身份证号
					Deposit = data.DepositRequired ? storeMenus.Deposit : 0,// 押金
					Rent = storeMenus.Rent,// 租金
					DispatchFee = await _configService.GetDispatchFeeAsync(),// 使用从数据库读取的 调度费
					Status = Order.EOrderStatus.待付款,
					CreatedAt = DateTime.Now,
					UpdatedAt = DateTime.Now
				};

				//_logger.LogDebug("订单创建信息{OrderId}", order.Id);

				//await _orderController.AddOrder(order);
				await _dbContext.Order.AddAsync(order);
				await _dbContext.SaveChangesAsync();
				await transaction.CommitAsync();

				_logger.LogInformation("订单{OrderId}创建", order.Id);

				// 获取新建订单的id
				return StatusCode(201, order.Id);

			} catch (Exception ex) {
				_logger.LogError(ex, "创建订单错误");
				await transaction.RollbackAsync();

				return StatusCode(403, "创建订单失败，请联系管理员");
			}
		}
		#endregion

		#region 支付
		/// <summary>
		/// 支付接口，支付成功后更改订单状态
		/// </summary>
		/// <returns></returns>
		[HttpPost("pay")]
		public async Task<IActionResult> PayOrder(PayData data) {
			Order? order = await _dbContext.Order.SingleOrDefaultAsync(o => o.TheUser == GetUserIdInt() && o.Id == data.OrderId);
			//Order? order = await _orderController.GetById(GetUserIdInt(), data.orderId);

			if (order is null) {
				return StatusCode(404, "订单不存在");
			}

			if (order.Status == Order.EOrderStatus.侍补余) {
				// 创建补余订单
				var so = await CreateSupplementaryOrders(order);
				if (so is null) {
					return StatusCode(500);
				}

				return StatusCode(204, new ReturnSupplementaryOrder(so));
			}

			if (order.Status != Order.EOrderStatus.待付款) {
				return StatusCode(403, "订单状态异常");
			}

			var now = DateTime.Now;

			if (order.CreatedAt > now.AddMinutes(10)) {
				return StatusCode(403, "订单已超时，请重新下单");
			}

			/* 订单数据定义 */
			string appid = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
			string mchid = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId;
			// 商品描述
			string description = "测试";
			// 商户订单号
			string outTradeNo = order.OutTradeNo!;// 创建订单号
												  //string outTradeNo = string.Concat("Rental_", Guid.NewGuid().ToString("N").AsSpan(0, 20), order.Id.ToString());// 创建订单号
												  // 交易结束时间（10分钟）
			string time_expire = order.CreatedAt.AddMinutes(10).ToString("yyyy-MM-ddTHH:mm:sszzz");
			// 附加数据
			string attach = "";
			// 通知地址
			const string notifyUrl = "https://wxcloudrun-dotnet-128645-8-1331625129.sh.run.tcloudbase.com/order/callback/notify";
			// 订单总金额（分）
			int total = Order.GetTotal(order.GetTotalPrice());
			// 用户Unionid
			var user = await _dbContext.User.SingleAsync(u => u.Id == GetUserIdInt());
			string unionid = user.Unionid;
			//string secret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
			//var sessionKey = await Senparc.Weixin.WxOpen.AdvancedAPIs.Sns.SnsApi.JsCode2JsonAsync(appid, secret, data.Code);
			//string unionid = sessionKey.unionid;

			// 创建请求类
			TransactionsRequestData requestData = new() {
				appid = appid,

				// 【直连商户号】 直连商户号
				mchid = mchid,

				// 【商品描述】 商品描述
				description = description,

				// 【商户订单号(string(32))】 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一。
				out_trade_no = outTradeNo,

				/// 选填
				/// 【交易结束时间】订单失效时间，遵循rfc3339标准格式，格式为yyyy-MM-DDTHH:mm:ss+TIMEZONE，yyyy-MM-DD表示年月日，
				/// T出现在字符串中，表示time元素的开头，HH:mm:ss表示时分秒，TIMEZONE表示时区（+08:00表示东八区时间，领先UTC8小时，即北京时间）。
				/// 例如：2015-05-20T13:29:35+08:00表示，北京时间2015年5月20日13点29分35秒。
				time_expire = time_expire,

				/// 选填
				/// 【附加数据】 附加数据，在查询API和支付通知中原样返回，可作为自定义参数使用，实际情况下只有支付完成状态才会返回该字段。
				attach = attach,

				// 【通知地址】 异步接收微信支付结果通知的回调地址，通知URL必须为外网可访问的URL，不能携带参数。
				// 公网域名必须为HTTPS，如果是走专线接入，使用专线NAT IP或者私有回调域名可使用HTTP
				notify_url = notifyUrl,

				// 【订单优惠标记】 订单优惠标记
				goods_tag = "",

				// 【订单金额】 订单金额信息
				amount = new TransactionsRequestData.Amount {
					// 【总金额】 订单总金额，单位为分。
					total = total,
					// 【货币类型】 CNY：人民币，境内商户号仅支持人民币。
					currency = "CNY"
				},

				// 【支付者】 支付者信息。
				payer = new TransactionsRequestData.Payer {
					// 【用户标识】 用户在普通商户AppID下的唯一标识。 下单前需获取到用户的OpenID，详见OpenID获取
					openid = unionid
				},

				/// 选填
				/// 【优惠功能】 优惠功能
				detail = null,

				/// 选填
				/// 【结算信息】 结算信息
				settle_info = null,

				/// 选填
				/// 【场景信息】 支付场景描述
				scene_info = null,

				/// 选填
				/// 【电子发票入口开放标识】 传入true时，支付成功消息和支付详情页将出现开票入口。需要在微信支付商户平台或微信公众平台开通电子发票功能，传此字段才可生效。
				support_fapiao = false
			};

			// 发起创建订单请求
			BasePayApis basePayApis = new();
			JsApiReturnJson result;
			try {
				result = await basePayApis.JsApiAsync(requestData);
			} catch (Exception ex) {
				Console.WriteLine($"下单失败：{ex.Message}");
				_logger.LogError(ex, "下单失败");
				return StatusCode(500);
			}

			// 【预支付交易会话标识】 预支付交易会话标识。用于后续接口调用中使用，该值有效期为2小时
			string prepayId = result.prepay_id;

			#region 加密
			if (prepayId.IsNullOrEmpty()) {
				//_logger.LogError("getdata{}", data);
				_logger.LogError("[PayOrder]创建订单错误result:{result}", result.ToJson(true));
				_logger.LogError("[PayOrder]订单信息requestData:{requestData}", requestData);
				return StatusCode(500);
			}

			//string appid = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
			long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			string nonceStr = Guid.NewGuid().ToString("N");
			string pack = "prepay_id=" + prepayId;
			string signType = "RSA";
			String paySign = GetSign(appid, timestamp, nonceStr, pack);//签名

			if (result.VerifySignSuccess != true) {
				_logger.LogError("获取 prepay_id 结果校验出错！");
				return StatusCode(403, "获取 prepay_id 结果校验出错！");
				throw new WeixinException("获取 prepay_id 结果校验出错！");
			}

			////获取 UI 信息包
			//var jsApiUiPackage = TenPaySignHelper.GetJsApiUiPackage(TenPayV3Info.AppId, result.prepay_id);
			//ViewData["jsApiUiPackage"] = jsApiUiPackage;

			////临时记录订单信息，留给退款申请接口测试使用（分布式情况下请注意数据同步）
			//HttpContext.Session.SetString("BillNo", sp_billno);
			//HttpContext.Session.SetString("BillFee", price.ToString());
			#endregion

			return StatusCode(200, new { appid, timestamp, nonceStr, pack, signType, paySign });
		}
		#endregion

		#region 创建补余订单
		/// <summary>
		/// 创建补余订单
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		private async Task<SupplementaryOrders?> CreateSupplementaryOrders(Order order) {
			/* 订单数据定义 */
			var now = DateTime.Now;

			// 创建补充订单
			SupplementaryOrders supplementaryOrders = new() {
				TheOrder = order.Id,
				OutTradeNo = string.Concat("SRental_", Guid.NewGuid().ToString("N").AsSpan(0, 20)),
				//TransactionId = "",//等
				Total = order.GetTotalPrice() - order.Paid,
				Status = EOrderStatus.待付款,
				//SuccessTime
				CreatedAt = now,
				UpdatedAt = now
			};


			try {
				await _dbContext.SupplementaryOrders.AddAsync(supplementaryOrders);
				await _dbContext.SaveChangesAsync();

			} catch (Exception e) {
				_logger.LogError(e, "创建补余订单失败");
				return null;
			}

			return supplementaryOrders;
		}
		#endregion

		#region 补余支付
		/// <summary>
		/// 补余支付
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		/// <exception cref="WeixinException"></exception>
		[HttpPost("pay/supplementaryOrder")]
		public async Task<IActionResult> PaySupplementaryOrder(PaySupplementaryOrderData data) {
			var supplementaryOrders = await _dbContext.SupplementaryOrders.SingleOrDefaultAsync(o => o.Id == data.SupplementaryOrderId);

			if (supplementaryOrders is null) {
				return StatusCode(404, "找不到");
			}

			/* 订单数据定义 */
			var now = DateTime.Now;

			/* 订单数据定义 */
			string appid = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
			string mchid = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId;
			// 商品描述
			string description = "补全剩余价格";
			// 商户订单号
			//string outTradeNo = order.OutTradeNo!;// 创建订单号
			string outTradeNo = supplementaryOrders.OutTradeNo!;// 创建订单号
																// 交易结束时间（10分钟）
			string time_expire = supplementaryOrders.CreatedAt.AddMinutes(10).ToString("yyyy-MM-ddTHH:mm:sszzz");
			// 附加数据
			string attach = "";
			// 通知地址
			const string notifyUrl = "https://wxcloudrun-dotnet-128645-8-1331625129.sh.run.tcloudbase.com/storeAccount/callback/notify";
			// 订单总金额（分）
			int total = Order.GetTotal(supplementaryOrders.Total);
			// 用户Unionid
			var user = await _dbContext.User.SingleAsync(u => u.Id == GetUserIdInt());
			string unionid = user.Unionid;
			//string secret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
			//var sessionKey = await Senparc.Weixin.WxOpen.AdvancedAPIs.Sns.SnsApi.JsCode2JsonAsync(appid, secret, data.Code);
			//string unionid = sessionKey.unionid;

			// 创建请求类
			TransactionsRequestData requestData = new() {
				appid = appid,

				// 【直连商户号】 直连商户号
				mchid = mchid,

				// 【商品描述】 商品描述
				description = description,

				// 【商户订单号(string(32))】 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一。
				out_trade_no = outTradeNo,

				/// 选填
				/// 【交易结束时间】订单失效时间，遵循rfc3339标准格式，格式为yyyy-MM-DDTHH:mm:ss+TIMEZONE，yyyy-MM-DD表示年月日，
				/// T出现在字符串中，表示time元素的开头，HH:mm:ss表示时分秒，TIMEZONE表示时区（+08:00表示东八区时间，领先UTC8小时，即北京时间）。
				/// 例如：2015-05-20T13:29:35+08:00表示，北京时间2015年5月20日13点29分35秒。
				time_expire = time_expire,

				/// 选填
				/// 【附加数据】 附加数据，在查询API和支付通知中原样返回，可作为自定义参数使用，实际情况下只有支付完成状态才会返回该字段。
				attach = attach,

				// 【通知地址】 异步接收微信支付结果通知的回调地址，通知URL必须为外网可访问的URL，不能携带参数。
				// 公网域名必须为HTTPS，如果是走专线接入，使用专线NAT IP或者私有回调域名可使用HTTP
				notify_url = notifyUrl,

				// 【订单优惠标记】 订单优惠标记
				goods_tag = "",

				// 【订单金额】 订单金额信息
				amount = new TransactionsRequestData.Amount {
					// 【总金额】 订单总金额，单位为分。
					total = total,
					// 【货币类型】 CNY：人民币，境内商户号仅支持人民币。
					currency = "CNY"
				},

				// 【支付者】 支付者信息。
				payer = new TransactionsRequestData.Payer {
					// 【用户标识】 用户在普通商户AppID下的唯一标识。 下单前需获取到用户的OpenID，详见OpenID获取
					openid = unionid
				},

				/// 选填
				/// 【优惠功能】 优惠功能
				detail = null,

				/// 选填
				/// 【结算信息】 结算信息
				settle_info = null,

				/// 选填
				/// 【场景信息】 支付场景描述
				scene_info = null,

				/// 选填
				/// 【电子发票入口开放标识】 传入true时，支付成功消息和支付详情页将出现开票入口。需要在微信支付商户平台或微信公众平台开通电子发票功能，传此字段才可生效。
				support_fapiao = false
			};

			// 发起创建订单请求
			BasePayApis basePayApis = new();
			JsApiReturnJson result;
			try {
				result = await basePayApis.JsApiAsync(requestData);
			} catch (Exception ex) {
				Console.WriteLine($"下单失败：{ex.Message}");
				_logger.LogError(ex, "[商家确认]下单失败");
				return StatusCode(500);
			}

			// 【预支付交易会话标识】 预支付交易会话标识。用于后续接口调用中使用，该值有效期为2小时
			string prepayId = result.prepay_id;

			#region 加密
			if (prepayId.IsNullOrEmpty()) {
				//_logger.LogError("getdata{}", data);
				_logger.LogError("[PayOrder]创建订单错误result:{result}", result.ToJson(true));
				_logger.LogError("[PayOrder]订单信息requestData:{requestData}", requestData);
				return StatusCode(500);
			}

			//string appid = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
			long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			string nonceStr = Guid.NewGuid().ToString("N");
			string pack = "prepay_id=" + prepayId;
			string signType = "RSA";
			String paySign = OrderAPI.GetSign(appid, timestamp, nonceStr, pack);//签名

			if (result.VerifySignSuccess != true) {
				_logger.LogError("获取 prepay_id 结果校验出错！");
				return StatusCode(403, "获取 prepay_id 结果校验出错！");
				throw new WeixinException("获取 prepay_id 结果校验出错！");
			}
			#endregion

			return StatusCode(202, new { appid, timestamp, nonceStr, pack, signType, paySign });
		}
		#endregion

		#region 小程序查询订单支付状态
		[HttpPost("query/notify")]
		public async Task<IActionResult> QueryNotify(int orderId) {
			Order? order = await _orderController.GetById(GetUserIdInt(), orderId);// 根据Id获取对应的订单


			return StatusCode(200);
		}
		#endregion

		#region 接收支付回调
		/// <summary>
		/// 支付回调（侍测试）
		/// </summary>
		/// <returns></returns>
		[AllowAnonymous]// 允许匿名访问
		[HttpPost("callback/notify")]
		public async Task<IActionResult> PayNotifyUrl() {
			_logger.LogInformation("PayNotifyUrl收到微信支付回调");

			WxPayCallbackViewModel returnData = new();// 创建应答格式
			try {
				//获取微信服务器异步发送的支付通知信息
				TenPayNotifyHandler resHandler = new TenPayNotifyHandler(HttpContext);
				OrderReturnJson orderReturnJson = await resHandler.DecryptGetObjectAsync<OrderReturnJson>();

				//记录日志
				_logger.LogInformation("PayNotifyUrl收到微信支付回调：{data}", orderReturnJson.ToJson(true));
				Senparc.Weixin.WeixinTrace.SendCustomLog("PayNotifyUrl 接收到消息", orderReturnJson.ToJson(true));

				//演示记录 transaction_id，实际开发中需要记录到数据库，以便退款和后续跟踪
				// transaction_id 微信支付系统生成的订单号。
				//Order? order = await _orderController.GetById(GetUserIdInt(), int.Parse(orderReturnJson.out_trade_no));// 根据Id获取对应的订单
				Order order = await _dbContext.Order.SingleAsync(o => o.OutTradeNo == orderReturnJson.out_trade_no);

				if (order is null) {
					_logger.LogError("订单获取错误transaction_id：{transaction_id}", orderReturnJson.out_trade_no);
					throw new Exception("订单获取错误transaction_id");
				}

				//获取支付状态
				string trade_state = orderReturnJson.trade_state;

				//验证请求是否从微信发过来（安全）

				//验证可靠的支付状态
				if (orderReturnJson.VerifySignSuccess == true) {
					var now = DateTime.Now;
					//order.Status = Order.EOrderStatus.待确认;// 更改订单状态
					//order.Paid += orderReturnJson.amount.total / 100m;// 增加已付金额,在代码中将 `total` 转换为元
					order.TransactionId = orderReturnJson.transaction_id;// 赋值微信传入的id
																		 //order.UpdatedAt = now;

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
								return StatusCode(500, returnData);
							}

							/* 提示：
							* 1、直到这里，才能认为交易真正成功了，可以进行数据库操作，但是别忘了返回规定格式的消息！
							* 2、上述判断已经具有比较高的安全性以外，还可以对访问 IP 进行判断进一步加强安全性。
							* 3、下面演示的是发送支付成功的模板消息提示，非必须。
							*/
							using (var transaction = await _dbContext.Database.BeginTransactionAsync()) {
								try {
									//_logger.LogInformation("开始更改订单状态");
									order.Status = Order.EOrderStatus.待确认;// 更改订单状态
									order.Paid += orderReturnJson.amount.total / 100m;// 增加已付金额
									order.SuccessTime = orderReturnJson.success_time;
									order.UpdatedAt = now;
									await _dbContext.SaveChangesAsync();

									//_logger.LogInformation("开始更改车辆状态");
									//Vehicle vehicle = await _dbContext.Vehicle.SingleAsync(v => v.Id == order.TheVehicle);
									//vehicle.State = Vehicle.Estates.已出租;
									//vehicle.UpdatedAt = now;
									//vehicle.StateUpdatedAt = now;
									//await _dbContext.SaveChangesAsync();

									await transaction.CommitAsync();
									_logger.LogInformation("更改订单状态完毕");
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
									order.Status = Order.EOrderStatus.已取消;
									order.UpdatedAt = now;

									Vehicle vehicle = await _dbContext.Vehicle.SingleAsync(v => v.Id == order.TheVehicle);
									vehicle.State = Vehicle.Estates.空闲;
									vehicle.StateUpdatedAt = now;
									vehicle.UpdatedAt = now;

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

		#region 订单退款请求
		/// <summary>
		/// 订单退款
		/// </summary>
		/// <param name="getData"></param>
		/// <returns></returns>
		[HttpPost("refund")]
		public async Task<IActionResult> OrderRefund(GetCancelReplacementInfo getData) {
			Order? order = await _orderController.GetById(GetUserIdInt(), getData.OrderId);

			// 待确认直接退款
			if (order is null || order.Status != Order.EOrderStatus.待确认)
				return StatusCode(403, "非法请求");

			//if (await _dbContext.RefundOrder.AnyAsync(ro => ro.TheOrder == order.Id))
			//    return StatusCode(403, "请勿重复请求");

			/* 退款流程 */
			var now = DateTime.Now;

			// 新建退款表数据
			RefundOrder refundOrder = new() {
				TheOrder = order.Id,
				//outRefundNo = string.Concat("Refund_", Guid.NewGuid().ToString("N").AsSpan(0, 20)),
				RefundId = "",//等待
				Reason = "直接退款",
				Status = RefundOrder.Estatus.已创建,//等待
				Total = order.Paid,
				Refund = order.Paid,
				SuccessTime = null,//等待
				CreateTime = now,//等待
				UpdatedAt = now,
			};

			RefundReturnJson refundReturnJson = await RefundAsync(order, refundOrder);// 调用退款

			if (refundReturnJson.ResultCode.Success != true) {
				_logger.LogError("退款请求失败{ResultCode}", refundReturnJson.ResultCode.ToJson(true));
				return StatusCode(403, "退款请求失败，请稍后再试");
			}

			try {
				// 生成退款表数据
				_dbContext.RefundOrder.Add(refundOrder);
				await _dbContext.SaveChangesAsync();

			} catch (Exception e) {
				_logger.LogError(e, "新建退款表数据失败{data}", refundOrder.ToJson(true));
				return StatusCode(500);
			}

			refundOrder.RefundId = refundReturnJson.refund_id;

			now = DateTime.Now;
			refundOrder.Status = (RefundOrder.Estatus)RefundOrderEstatusHashtable[refundReturnJson.status]!;// 获取对应枚举值
			refundOrder.SuccessTime = refundReturnJson.success_time;
			refundOrder.CreateTime = DateTimeOffset.Parse(refundReturnJson.create_time).UtcDateTime;//【退款创建时间】退款受理时间
			refundOrder.UpdatedAt = now;

			_dbContext.RefundOrder.Update(refundOrder);

			try {
				await _dbContext.SaveChangesAsync();

			} catch (Exception e) {
				_logger.LogError(e, "退款表更新失败refundOrder：{refundOrder}", refundOrder.ToJson(true));
				return StatusCode(500);
			}

			_logger.LogInformation("退款已成功refundReturnJson：{refundReturnJson}", refundReturnJson.ToJson(true));


			using var transaction = await _dbContext.Database.BeginTransactionAsync();// 事务开始

			// 更改订单信息
			order.Status = Order.EOrderStatus.退款中;
			order.UpdatedAt = now;

			Vehicle vehicle = await _dbContext.Vehicle.SingleAsync(v => v.Id == order.TheVehicle);
			vehicle.State = Vehicle.Estates.空闲;

			try {
				await _dbContext.SaveChangesAsync();
				await transaction.CommitAsync();

			} catch (Exception e) {
				_logger.LogError(e, "订单退款更新失败order：{OrderId}", order.Id);
				await transaction.RollbackAsync();
				return StatusCode(500);
			}

			return StatusCode(200);
		}
		#endregion

		#region 退款回调
		[AllowAnonymous]// 允许匿名访问
		[HttpPost("callback/refund")]
		public async Task<IActionResult> RefundNotify(GetCancelReplacementInfo getData) {
			_logger.LogInformation("RefundNotify收到微信退款回调");

			WeixinTrace.SendCustomLog("RefundNotifyUrl被访问", "IP" + HttpContext.UserHostAddress()?.ToString());

			WxPayCallbackViewModel returnData = new();
			try {
				var resHandler = new TenPayNotifyHandler(HttpContext);
				var refundNotifyJson = await resHandler.DecryptGetObjectAsync<RefundNotifyJson>();

				WeixinTrace.SendCustomLog("跟踪RefundNotifyUrl信息", refundNotifyJson.ToJson(true));
				_logger.LogInformation("refundNotifyJson:{refundNotifyJson}", refundNotifyJson.ToJson(true));

				string refund_status = refundNotifyJson.refund_status;
				//if (refundNotifyJson.VerifySignSuccess != true)
				if (/*refundNotifyJson.VerifySignSuccess == true &*/ /*refund_status == "SUCCESS"*/ true) {

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

					switch (refund_status) {
						case "SUCCESS":
							using (var transaction = await _dbContext.Database.BeginTransactionAsync()) {
								try {
									var now = DateTime.Now;
									_logger.LogInformation("开始更新退款相关数据");

									var refundOrder = await _dbContext.RefundOrder.SingleAsync(ro => ro.RefundId == refundNotifyJson.out_refund_no);
									refundOrder.Status = (RefundOrder.Estatus)RefundOrderEstatusHashtable[refundNotifyJson.refund_status]!;
									refundOrder.SuccessTime = DateTimeOffset.Parse(refundNotifyJson.success_time).UtcDateTime;// 退款成功时间
									refundOrder.UpdatedAt = now;
									await _dbContext.SaveChangesAsync();

									Order order = await _dbContext.Order.SingleAsync(o => o.Id == refundOrder.TheOrder);
									order.Status = Order.EOrderStatus.已退款;
									order.UpdatedAt = now;
									await _dbContext.SaveChangesAsync();

									Vehicle vehicle = await _dbContext.Vehicle.SingleAsync(v => v.Id == order.TheVehicle);
									vehicle.State = Vehicle.Estates.空闲;
									vehicle.StateUpdatedAt = now;
									vehicle.UpdatedAt = now;
									await _dbContext.SaveChangesAsync();

									_logger.LogInformation("相关数据refundOrder更新{refundOrderId}", refundOrder.Id);

									await transaction.CommitAsync();

								} catch (Exception e) {
									await transaction.RollbackAsync();
									_logger.LogError(e, "退款回调refundOrder失败");
									returnData.code = "FAILD";
									returnData.message = "数据库更新错误";
									return StatusCode(500, returnData);
								}
							}
							break;

						default:
							_logger.LogInformation("退款收到其他状态{refund_status}", refund_status);
							break;
					}

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
				_logger.LogError(ex, "退款回调发生错误");
				WeixinTrace.WeixinExceptionLog(new WeixinException(ex.Message, ex));
				return StatusCode(500, returnData);
			}

			//https://pay.weixin.qq.com/wiki/doc/apiv3/wechatpay/wechatpay3_3.shtml
			//return Json(returnData);

			//return StatusCode(200);
		}
		#endregion

		#region 取消订单
		/// <summary>
		/// 取消订单
		/// </summary>
		/// <param name="getData"></param>
		/// <returns></returns>
		[HttpPost("cancel")]
		public async Task<IActionResult> CancellationOrder(GetCancelOrder getData) {
			Order? order = await _orderController.GetById(GetUserIdInt(), getData.OrderId);

			if (order is null || order.Status != Order.EOrderStatus.待付款)
				return StatusCode(403, "非法请求");

			using var transaction = await _dbContext.Database.BeginTransactionAsync();

			try {
				var now = DateTime.Now;
				order.Status = Order.EOrderStatus.已取消;
				order.UpdatedAt = now;
				await _dbContext.SaveChangesAsync();

				// 车辆
				Vehicle vehicle = await _dbContext.Vehicle.SingleAsync(v => v.Id == order.TheVehicle);

				vehicle.State = Vehicle.Estates.空闲;
				vehicle.StateUpdatedAt = now;
				await _dbContext.SaveChangesAsync();

				await transaction.CommitAsync();

				// 关闭订单
				BasePayApis basePayApis = new();

				CloseRequestData closeRequestData = new(Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId, order.OutTradeNo);

				var re = await basePayApis.CloseOrderAsync(closeRequestData);

			} catch (Exception e) {
				_logger.LogError("取消订单{OrderId}", getData.OrderId);
				await transaction.RollbackAsync();
				return StatusCode(500);
			}

			return StatusCode(200);
		}
		#endregion

		#region 换车请求
		/// <summary>
		/// 换车请求
		/// </summary>
		/// <param name="getData"></param>
		/// <returns></returns>
		[HttpPost("replacement")]
		public async Task<IActionResult> Replacement(GetReplacementInfo getData) {
			// 检查订单状态
			Order? order = await _orderController.GetById(GetUserIdInt(), getData.OrderId);
			if (order is null || order.Status != Order.EOrderStatus.进行中) {
				_logger.LogDebug("订单{OrderId}状态非法", getData.OrderId);
				return StatusCode(403, "订单不存在或非法");
			}

			// 检查是否存在换车请求
			if (await _orderController.GetOderReplacementByUserId(GetUserIdInt(), getData.OrderId))
				return StatusCode(403, "当前有侍确认的换车请求");

			// 检查车辆状态
			Vehicle? vehicle = await _dbContext.Vehicle.SingleOrDefaultAsync(v => v.Id == getData.ReplacementVehicleId);
			if (vehicle is null || vehicle.State != Vehicle.Estates.空闲)
				return StatusCode(403, "车辆不存在或状态非法");

			if (vehicle.TheCurrentStore is null)
				return StatusCode(403, "车辆门店状态异常");

			using var transaction = await _dbContext.Database.BeginTransactionAsync();// 事务开始

			// 锁定车辆
			vehicle.State = Vehicle.Estates.锁定;

			// 添加至换车表
			VehicleReplacementRecord vrr = new() {
				TheOrder = getData.OrderId,// 订单Id
				TheOldVehicles = order.TheVehicle,// 旧车辆
				TheNewVehicles = getData.ReplacementVehicleId,// 要更换的车辆
				TheStore = (int)vehicle.TheCurrentStore,
				State = VehicleReplacementRecord.Estates.侍确认,// 状态
				CreatedAt = DateTime.Now
			};
			try {
				await _dbContext.VehicleReplacementRecord.AddAsync(vrr);
				await _dbContext.SaveChangesAsync();
				await transaction.CommitAsync();// 提交事务

			} catch (Exception e) {
				_logger.LogError(e, "VehicleReplacementRecord：{VehicleReplacementRecordId}添加至换车表", vrr.Id);
				await transaction.RollbackAsync();// 回滚
				return StatusCode(500);

			}

			return StatusCode(200, "请求成功，请向商家确认");
		}
		#endregion

		#region 取消换车请求
		/// <summary>
		/// 取消换车请求
		/// </summary>
		/// <param name="getReplacementVehicle"></param>
		/// <returns></returns>
		[HttpPost("replacement/cancel")]
		public async Task<IActionResult> CancelReplacement(GetCancelReplacementInfo getData) {
			// 检查订单状态
			Order? order = await _orderController.GetById(GetUserIdInt(), getData.OrderId);

			if (order is null || order.Status != Order.EOrderStatus.进行中) {
				_logger.LogDebug("订单{OrderId}状态非法", getData.OrderId);
				return StatusCode(403, "订单不存在或非法");
			}

			// 检查是否存在换车请求
			if (await _orderController.GetOderReplacementByUserId(GetUserIdInt(), getData.OrderId))
				return StatusCode(403, "当前有侍确认的换车请求");

			VehicleReplacementRecord? vrr = await _dbContext.VehicleReplacementRecord.FirstOrDefaultAsync(vrr => vrr.TheOrder == getData.OrderId && vrr.State == VehicleReplacementRecord.Estates.侍确认);
			if (vrr is null)
				return StatusCode(403, "请求异常");

			vrr.State = VehicleReplacementRecord.Estates.已取消;

			try {
				await _dbContext.SaveChangesAsync();

			} catch (Exception e) {
				_logger.LogCritical(e, "取消换车请求{VehicleReplacementRecordId}", vrr.Id);
				return StatusCode(500);

			}

			return StatusCode(200);
		}
		#endregion

		#region 还车（不使用）
		private async Task<IActionResult> ReturnVehicle(GetReturnInfo getData) {
			// 检查订单状态
			Order? order = await _orderController.GetById(GetUserIdInt(), getData.OrderId);
			if (order is null || order.Status != Order.EOrderStatus.进行中) {
				_logger.LogDebug("订单{OrderId}状态非法", getData.OrderId);
				return StatusCode(403, "订单不存在或非法");
			}

			// 检查是否存在换车请求
			if (await _orderController.GetOderReplacementByUserId(GetUserIdInt(), getData.OrderId))
				return StatusCode(403, "当前有侍确认的换车请求");

			// 检查还车点
			Store? store = await _dbContext.Store.SingleOrDefaultAsync(s => s.Id == getData.StoreId);

			if (store is null || !(store.IsOpen())) {
				return StatusCode(403, "请检查还车点");
			}

			// 当前时间
			var now = DateTime.Now;

			// 超时费率 (每小时10元)
			decimal overtimeRate = 10;

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
			if (order.TheRentalLocation != getData.StoreId) {
				order.DispatchFee += 10;
			}

			#region 退款
			// 退押金金额（金额修改完成后计算！！！）
			var sum = order.Paid - order.GetTotalPrice();

			// 新建退款表数据
			RefundOrder refundOrder = new() {
				TheOrder = order.Id,
				RefundId = "",//等待
				Reason = "自动退款",
				Status = RefundOrder.Estatus.已创建,//等待
				Total = Order.GetTotal(order.Paid),
				Refund = Order.GetTotal(sum),//归还多余费用
				SuccessTime = null,//等待
				CreateTime = now,//等待
				UpdatedAt = now,
			};

			using var transaction = await _dbContext.Database.BeginTransactionAsync();

			try {
				Vehicle vehicle = await _dbContext.Vehicle.SingleAsync(v => v.Id == order.TheVehicle);
				vehicle.State = Vehicle.Estates.侍确认;
				vehicle.StateUpdatedAt = now;

				// 生成退款表数据
				_dbContext.RefundOrder.Add(refundOrder);

				// 调用退款服务
				RefundReturnJson refundReturnJson = await RefundAsync(order, refundOrder);

				order.Status = Order.EOrderStatus.退款中;
				order.ActualReturnTime = now;// 归还时间
				order.TheReturnThePoint = store.Id;
				order.DepositRefunded += refundOrder.Refund;
				order.UpdatedAt = now;

				// 营业额统计表
				RevenueStatistics revenueStatistics = new() {
					TheStoreA = order.TheRentalLocation,
					TheStoreB = (int)order.TheReturnThePoint,
					TheOrder = order.Id,
					CreatedAt = now,
					UpdatedAt = now,
				};

				await _dbContext.RevenueStatistic.AddAsync(revenueStatistics);
				await _dbContext.SaveChangesAsync();

				// 如果退款成功，提交事务
				await transaction.CommitAsync();
			} catch (Exception ex) {
				_logger.LogError(ex, "订单处理失败。订单ID: {OrderId}, 车辆ID: {VehicleId}, 退款金额: {RefundAmount}",
					order.Id, order.TheVehicle, refundOrder.Refund);
				await transaction.RollbackAsync();
				return StatusCode(500);
			}
			#endregion

			return StatusCode(200);
		}
		#endregion

		#region 检查换车请求，超时取消
		/// <summary>
		/// 检查换车请求，超时取消
		/// </summary>
		/// <returns></returns>
		public async Task CheckReplacementVehicle() {
			DateTime now = DateTime.Now;// 获取当前时间
			DateTime threshold = now.AddMinutes(-30);// 计算30分钟前的时间

			List<VehicleReplacementRecord> vrrToCancel = await _dbContext.VehicleReplacementRecord
				.Where(vrr => vrr.State == VehicleReplacementRecord.Estates.侍确认 && vrr.CreatedAt < threshold)
				.ToListAsync();

			using var transaction = await _dbContext.Database.BeginTransactionAsync();// 事务开始

			foreach (VehicleReplacementRecord v in vrrToCancel) {
				try {
					// 更新订单状态
					v.State = VehicleReplacementRecord.Estates.已取消;
					v.UpdatedAt = now;

					// 查询并更新与订单关联的车辆状态为空闲
					Vehicle vehicle = await _dbContext.Vehicle
						.SingleAsync(vehicle => vehicle.Id == v.TheNewVehicles);

					vehicle.State = Vehicle.Estates.空闲;
					vehicle.UpdatedAt = now;

					_logger.LogInformation("订单{OrderId}换车取消, 车辆{VehicleId}状态更新为空闲", v.Id, vehicle.Id);

					// 尝试保存更改
					await _dbContext.SaveChangesAsync();

					await transaction.CommitAsync();
				} catch (DbUpdateConcurrencyException ex) {
					_logger.LogWarning(ex, "并发冲突，无法更新订单{OrderId}或车辆{VehicleId}状态", v.Id, v.TheNewVehicles);

					// 可选择：重新查询、通知用户或跳过冲突条目
				} catch (Exception ex) {
					_logger.LogCritical(ex, "更新订单{OrderId}或车辆{VehicleId}状态失败", v.Id, v.TheNewVehicles);

					await transaction.RollbackAsync();

				}
			}
		}
		#endregion

		#region 检查未支付订单并取消超过10分钟的订单
		/// <summary>
		/// 检查未支付订单并取消超过10分钟的订单
		/// </summary>
		/// <returns></returns>
		public async Task CheckOrderPayment() {
			//using MyDbContext _dbContext = new();
			DateTime now = DateTime.Now;// 获取当前时间
			DateTime threshold = now.AddMinutes(-10);// 计算10分钟前的时间

			// 查询所有超过10分钟未支付的待付款订单
			List<Order> ordersToCancel = new();
			try {
				ordersToCancel = await _dbContext.Order
					.Where(o => o.Status == Order.EOrderStatus.待付款 && o.CreatedAt < threshold)
					.ToListAsync();

			} catch (MySqlException e) {
				if (e.ErrorCode.Equals(9449)) {
					_logger.LogWarning("MySqlConnector.MySqlException:“CynosDB serverless instance is resuming, please try connecting again”");
					return;
				} else {
					_logger.LogCritical(e, "获取待付款订单{ordersToCancel}", ordersToCancel.ToArray());
					throw;
				}
			}

			using var transaction = await _dbContext.Database.BeginTransactionAsync();// 事务开始

			foreach (var order in ordersToCancel) {
				try {
					order.Status = Order.EOrderStatus.已取消;
					order.UpdatedAt = now;

					Vehicle vehicle = await _dbContext.Vehicle
						.SingleAsync(v => v.Id == order.TheVehicle);

					vehicle.State = Vehicle.Estates.空闲;
					vehicle.UpdatedAt = now;

					_logger.LogInformation("订单{OrderId}取消, 车辆{VehicleId}状态更新为空闲", order.Id, vehicle.Id);

					await _dbContext.SaveChangesAsync();
					await transaction.CommitAsync();

				} catch (DbUpdateConcurrencyException ex) {
					_logger.LogWarning("并发冲突，订单或车辆记录已被更新：{OrderId}, {Exception}", order.Id, ex);

				} catch (Exception ex) {
					_logger.LogCritical(ex, "订单或车辆状态事务失败，ordersToCancel：{OrdersToCancel}",
						ordersToCancel.Select(o => o.Id).ToArray());

					await transaction.RollbackAsync();
					throw;
				}
			}
			return;
		}
		#endregion

		#region 订单评价
		/// <summary>
		/// 添加订单评价
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[HttpPost("valuate")]
		public async Task<IActionResult> AddOrderEvaluate(Getvaluate data) {
			if (!(data.Score > 0 && data.Score <= 10)) {
				return StatusCode(403, "请填写正确的评分（1-10分）");
			}

			Order? order = await _orderController.GetById(GetUserIdInt(), data.TheOrder);

			if (order is null) {
				return StatusCode(403, "未找到订单");
			}

			if (order.Status != EOrderStatus.已完成 && order.Status != EOrderStatus.已补余 && order.Status != EOrderStatus.已退款) {
				return StatusCode(403, "请在订单完成后评价");
			}

			if (await _dbContext.OrderEvaluate.AnyAsync(oe => oe.TheOrder == data.TheOrder)) {
				return StatusCode(403, "请勿重复评价");
			}

			OrderEvaluate orderEvaluate = new() {
				TheOrder = data.TheOrder,
				Score = data.Score,
				Notes = data.Notes,
				CreatedAt = DateTime.Now
			};

			try {
				await _dbContext.OrderEvaluate.AddAsync(orderEvaluate);
				await _dbContext.SaveChangesAsync();

			} catch (Exception e) {
				_logger.LogError(e, "新增订单评价失败OrderId:{OrderId}", data.TheOrder);
				return StatusCode(500, "评价失败，请稍后再试");
			}

			return StatusCode(201);
		}
		#endregion

		/// <summary>
		/// 退款
		/// </summary>
		/// <param name="order"></param>
		/// <param name="refundOrder"></param>
		/// <returns></returns>
		public async static Task<RefundReturnJson> RefundAsync(Order order, RefundOrder refundOrder) {
			/* 退款流程 */
			var now = DateTime.Now;

			//// 新建退款表数据
			//RefundOrder refundOrder = new() {
			//	TheOrder = order.Id,
			//	RefundId = "",//等待
			//	Reason = "直接退款",
			//	Status = RefundOrder.Estatus.已创建,//等待
			//	Total = Order.GetTotal(order.Paid),
			//	Refund = Order.GetTotal(order.Paid),
			//	SuccessTime = null,//等待
			//	CreateTime = now,//等待
			//	UpdatedAt = now,
			//};

			//// 生成退款表数据
			//_dbContext.RefundOrder.Add(refundOrder);

			//try {
			//	await _dbContext.SaveChangesAsync();

			//} catch (Exception e) {
			//	_logger.LogError(e, "新建退款表数据");
			//	return StatusCode(500);
			//}

			BasePayApis basePayApis = new();

			//【微信支付订单号】原支付交易对应的微信订单号，与out_trade_no二选一
			string transaction_id = order.TransactionId;
			//【商户订单号】原支付交易对应的商户订单号，与transaction_id二选一
			string out_trade_no = order.OutTradeNo;
			//【商户退款单号】商户系统内部的退款单号，商户系统内部唯一，只能是数字、大小写字母_-|*@ ，同一退款单号多次请求只退一笔。
			//string out_refund_no = string.Concat("Refund_", Guid.NewGuid().ToString("N").AsSpan(0, 20), refundOrder.Id.ToString());
			string out_refund_no = refundOrder.outRefundNo;

			//【退款原因】若商户传入，会在下发给用户的退款消息中体现退款原因
			string reason = refundOrder.Reason;
			//【退款币种】符合ISO 4217标准的三位字母代码，目前只支持人民币：CNY。
			string currency = "CNY";
			//【退款结果回调url】异步接收微信支付退款结果通知的回调地址，通知url必须为外网可访问的url，不能携带参数。 如果参数中传了notify_url，则商户平台上配置的回调地址将不会生效，优先回调当前传的这个地址。
			const string notify_url = "https://wxcloudrun-dotnet-128645-8-1331625129.sh.run.tcloudbase.com/order/callback/refund";
			RefundRequestData refundRequestData = new() {
				transaction_id = transaction_id,
				out_trade_no = out_trade_no,
				out_refund_no = out_refund_no,
				reason = reason,
				notify_url = notify_url,
				funds_account = null,
				amount = new RefundRequestData.Amount {
					//【退款金额】退款金额，单位为分，只能为整数，不能超过原订单支付金额。
					refund = Order.GetTotal(refundOrder.Refund),
					/// 选填【退款出资账户及金额】退款需要从指定账户出资时，传递此参数指定出资金额（币种的最小单位，只能为整数）。
					from = null,
					//【原订单金额】原支付交易的订单总金额，单位为分，只能为整数。
					total = Order.GetTotal(refundOrder.Total),
					currency = "CNY"
				},
				/// 选填【退款商品】指定商品退款需要传此参数，其他场景无需传递
				goods_detail = null
			};

			return await basePayApis.RefundAsync(refundRequestData);// 调用退款
		}

		/// <summary>
		/// JWT 获取用户id
		/// </summary>
		/// <returns></returns>
		public int GetUserIdInt() {
			return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
		}

		#region RSA加密用
		/// <summary>
		/// 获取签名
		/// </summary>
		/// <param name="appId"></param>
		/// <param name="timestamp">时间戳</param>
		/// <param name="nonceStr">随机字符串</param>
		/// <param name="pack"></param>
		/// <returns></returns>
		public static string GetSign(string appId, long timestamp, string nonceStr, string pack) {
			string message = BuildMessage(appId, timestamp, nonceStr, pack);
			string paySign = Sign(message);
			return paySign;
		}

		// 构建消息
		private static string BuildMessage(string appId, long timestamp, string nonceStr, string pack) {
			return $"{appId}\n{timestamp}\n{nonceStr}\n{pack}\n";
		}

		// 签名方法
		private static string Sign(string message) {
			// 获取商户私钥明文
			string privateKey = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_PrivateKey;

			privateKey = "-----BEGIN PRIVATE KEY-----" + privateKey + "-----END PRIVATE KEY-----";

			// 创建 RSA 对象并加载私钥
			using RSA rsa = RSA.Create();
			rsa.ImportFromPem(privateKey.ToCharArray());

			// 签名
			byte[] messageBytes = Encoding.UTF8.GetBytes(message);
			byte[] signedBytes = rsa.SignData(messageBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

			// Base64 编码签名
			return Convert.ToBase64String(signedBytes);
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

	public class Getvaluate {
		/// <summary>
		/// 订单编号
		/// </summary>
		public int TheOrder { get; set; }

		/// <summary>
		/// 用户评分（1-10分）
		/// </summary>
		public int Score { get; set; }

		/// <summary>
		/// 备注
		/// </summary>
		public string? Notes { get; set; }
	}

	public class GetReturnInfo {
		/// <summary>
		/// 还车点
		/// </summary>
		public int StoreId { get; set; }
		public int OrderId { get; internal set; }
	}

	/// <summary>
	/// 下单用
	/// </summary>
	public class PayData {
		public int OrderId { get; set; }
	}

	/// <summary>
	/// 下单用
	/// </summary>
	public class PaySupplementaryOrderData {
		public int SupplementaryOrderId { get; set; }
	}

	/// <summary>
	/// 获取创建订单信息
	/// </summary>
	public class GetOrder {
		public int StoreMenuId { get; set; }// 套餐Id
		public bool DepositRequired { get; set; }// 需要押金
		public string UserName { get; set; }// 用户姓名
		public string UserPhone { get; set; }// 用户手机号
		public string IdentityCard { get; set; }// 身份证号
		public int Vehicle { get; set; }// 租用车辆
	}

	/// <summary>
	/// 返回给微信支付回调结果的实体类
	/// </summary>
	public class WxPayCallbackViewModel {
		/// <summary>
		/// 返回状态码,错误码，SUCCESS为清算机构接收成功，其他错误码为失败。
		/// </summary>
		public string code { set; get; } = "SUCCESS";

		/// <summary>
		/// 返回信息，如非空，为错误原因。
		/// </summary>
		public string message { set; get; } = string.Empty;
	}

	public class Resource {
		public string OriginalType { get; set; }
		public string Algorithm { get; set; }
		public string Ciphertext { get; set; }
		public string AssociatedData { get; set; }
		public string Nonce { get; set; }
	}

	/// <summary>
	/// 获取换车请求信息
	/// </summary>
	public class GetReplacementInfo {
		public int OrderId { get; set; }
		public int ReplacementVehicleId { get; set; }// 更改车辆Id
	}

	/// <summary>
	/// 获取取消换车请求信息
	/// </summary>
	public class GetCancelReplacementInfo {
		public int OrderId { get; set; }
	}

	/// <summary>
	/// 获取取消订单请求信息
	/// </summary>
	public class GetCancelOrder {
		public int OrderId { get; set; }
	}

	#region 回调相关
	/// <summary>
	/// 微信支付结果回调通知实体
	/// </summary>
	public class WxPayNotifyModel {
		/// <summary>
		/// 通知的唯一ID
		/// </summary>
		public string id { set; get; }

		/// <summary>
		/// 通知创建时间,格式为YYYY-MM-DDTHH:mm:ss+TIMEZONE，YYYY-MM-DD表示年月日，T出现在字符串中，表示time元素的开头，HH:mm:ss.表示时分秒，TIMEZONE表示时区（+08:00表示东八区时间，领先UTC 8小时，即北京时间）。例如：2015-05-20T13:29:35+08:00表示北京时间2015年05月20日13点29分35秒。
		/// </summary>
		public string create_time { set; get; }

		/// <summary>
		/// 通知的类型，支付成功通知的类型为TRANSACTION.SUCCESS
		/// </summary>
		public string event_type { set; get; }

		/// <summary>
		/// 通知的资源数据类型，支付成功通知为encrypt-resource
		/// </summary>
		public string resource_type { set; get; }

		/// <summary>
		/// 通知资源数据,json格式
		/// </summary>
		public WxPayResourceModel resource { set; get; }

		/// <summary>
		/// 回调摘要
		/// </summary>
		public string summary { set; get; }
	}

	/// <summary>
	/// 微信支付回调通知结果resource实体
	/// </summary>
	public class WxPayResourceModel {
		/// <summary>
		/// 对开启结果数据进行加密的加密算法，目前只支持AEAD_AES_256_GCM
		/// </summary>
		public string algorithm { set; get; }

		/// <summary>
		/// Base64编码后的开启/停用结果数据密文
		/// </summary>
		public string ciphertext { set; get; }

		/// <summary>
		/// 附加数据
		/// </summary>
		public string associated_data { set; get; }

		/// <summary>
		/// 原始回调类型，为transaction
		/// </summary>
		public string original_type { set; get; }

		/// <summary>
		/// 加密使用的随机串
		/// </summary>
		public string nonce { set; get; }
	}

	/// <summary>
	/// 微信支付回调通知结果resource解密实体
	/// </summary>
	public class WxPayResourceDecryptModel {
		/// <summary>
		/// 直连商户申请的公众号或移动应用appid
		/// </summary>
		public string appid { set; get; }

		/// <summary>
		/// 商户的商户号，由微信支付生成并下发。
		/// </summary>
		public string mchid { set; get; }

		/// <summary>
		/// 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一。特殊规则：最小字符长度为6
		/// </summary>
		public string out_trade_no { set; get; }

		/// <summary>
		/// 微信支付系统生成的订单号。
		/// </summary>
		public string transaction_id { set; get; }

		/// <summary>
		/// 交易类型，枚举值：
		/// JSAPI：公众号支付
		/// NATIVE：扫码支付
		/// App：App支付
		/// MICROPAY：付款码支付
		/// MWEB：H5支付
		/// FACEPAY：刷脸支付
		/// </summary>
		public string trade_type { set; get; }

		/// <summary>
		/// 交易状态，枚举值：
		/// SUCCESS：支付成功
		/// REFUND：转入退款
		/// NOTPAY：未支付
		/// CLOSED：已关闭
		/// REVOKED：已撤销（付款码支付）
		/// USERPAYING：用户支付中（付款码支付）
		/// PAYERROR：支付失败(其他原因，如银行返回失败)
		/// ACCEPT：已接收，等待扣款
		/// </summary>
		public string trade_state { get; set; }

		/// <summary>
		/// 交易状态描述
		/// </summary>
		public string trade_state_desc { get; set; }

		/// <summary>
		/// 银行类型，采用字符串类型的银行标识。银行标识请参考《银行类型对照表》。
		/// </summary>
		public string bank_type { set; get; }

		/// <summary>
		/// 附加数据，在查询API和支付通知中原样返回，可作为自定义参数使用，实际情况下只有支付完成状态才会返回该字段。
		/// </summary>
		public string? attach { set; get; }

		/// <summary>
		/// 支付完成时间，遵循rfc3339标准格式，格式为yyyy-MM-DDTHH:mm:ss+TIMEZONE
		/// </summary>
		public string success_time { set; get; }

		/// <summary>
		/// 支付者信息
		/// </summary>
		public WxPayer payer { set; get; }

		/// <summary>
		/// 订单金额信息
		/// </summary>
		public WxAmount amount { set; get; }

		/// <summary>
		/// 支付场景信息描述
		/// </summary>
		public WxSceneInfo scene_info { set; get; }

		//public Promotion_Detail[] promotion_detail { get; set; }
	}
	/// <summary>
	/// 支付用户信息实体
	/// </summary>
	public class WxPayer {
		/// <summary>
		/// 用户在直连商户appid下的唯一标识。
		/// </summary>
		public string openid { get; set; }
	}

	/// <summary>
	/// 订单金额信息实体
	/// </summary>
	public class WxAmount {
		/// <summary>
		/// 订单总金额，单位为分。
		/// </summary>
		public int total { set; get; }

		/// <summary>
		/// 用户支付金额，单位为分。
		/// </summary>
		public int payer_total { set; get; }

		/// <summary>
		/// CNY：人民币，境内商户号仅支持人民币。
		/// </summary>
		public string currency { set; get; }

		/// <summary>
		/// 用户支付币种。
		/// </summary>
		public string payer_currency { set; get; }
	}

	/// <summary>
	/// 支付场景信息实体
	/// </summary>
	public class WxSceneInfo {
		public string device_id { set; get; }
	}


	#endregion

	/// <summary>
	/// 基础返回订单
	/// </summary>
	public struct ReturnOrderBasic {
		public ReturnOrderBasic(Order order) {
			OrderId = order.Id;
			TheRentalLocation = order.TheRentalLocation;
			TheVehicle = order.TheVehicle;
			Deposit = order.Deposit;
			Rent = order.Rent;
			DispatchFee = order.DispatchFee;
			OvertimeFee = order.OvertimeFee;
			OtherFees = order.OtherFees;
			Paid = order.Paid;
			DepositRefunded = order.DepositRefunded;
			Status = order.Status;
			CreatedAt = order.CreatedAt;
		}
		public int OrderId { get; init; }// 订单编号
		public int TheRentalLocation { get; set; }// 租车点（StoreId）
		public int TheVehicle { get; set; }// 租用车辆
		public decimal Deposit { get; set; }// 押金
		public decimal Rent { get; set; }// 租金
		public decimal DispatchFee { get; set; }// 调度费
		public decimal OvertimeFee { get; set; }// 超时费
		public decimal OtherFees { get; set; }// 其他费用
		public decimal Paid { get; set; }// 已付
		public decimal DepositRefunded { get; set; }// 已退押金
		public Order.EOrderStatus Status { get; set; }// 订单状态
		public DateTime CreatedAt { get; set; }
	}

	/// <summary>
	/// 详细返回订单格式
	/// </summary>
	public struct ReturnOrder {
		public ReturnOrder(Order order, StoreMenu? storeMenu) {
			OrderId = order.Id;
			ActualStartingTime = order.ActualStartingTime;
			ActualReturnTime = order.ActualReturnTime;
			TheStoreMenu = order.TheStoreMenu;
			TheVehicle = order.TheVehicle;
			TheRentalLocation = order.TheRentalLocation;
			TheReturnThePoint = order.TheReturnThePoint;
			UserName = order.UserName;
			UserPhone = order.UserPhone;
			Deposit = order.Deposit;
			Rent = order.Rent;
			DispatchFee = order.DispatchFee;
			OvertimeFee = order.OvertimeFee;
			OtherFees = order.OtherFees;
			Paid = order.Paid;
			DepositRefunded = order.DepositRefunded;
			Status = order.Status;
			SuccessTime = order.SuccessTime;
			Notes = order.Notes;
			CreatedAt = order.CreatedAt;

			RentalDuration = storeMenu is null ? 0 : storeMenu.Duration;
		}
		public int OrderId { get; init; }// 订单编号
		public DateTime? ActualStartingTime { get; set; }// 实际起始时间
		public DateTime? ActualReturnTime { get; set; }// 实际归还时间
		public int TheStoreMenu { get; set; }// 套餐
		public int TheVehicle { get; set; }// 租用车辆
		public int TheRentalLocation { get; set; }// 租车点（StoreId）
		public int? TheReturnThePoint { get; set; }// 还车点（StoreId）
		public string UserName { get; set; }// 用户姓名
		public string UserPhone { get; set; }// 用户手机号
		public decimal Deposit { get; set; }// 押金
		public decimal Rent { get; set; }// 租金
		public decimal DispatchFee { get; set; }// 调度费
		public decimal OvertimeFee { get; set; }// 超时费
		public decimal OtherFees { get; set; }// 其他费用
		public decimal Paid { get; set; }// 已付
		public decimal DepositRefunded { get; set; }// 已退押金
		public Order.EOrderStatus Status { get; set; }// 订单状态
		public DateTime? SuccessTime { get; set; }// 支付完成时间
		public string? Notes { get; set; }// 备注
		public DateTime CreatedAt { get; set; }

		public int RentalDuration { get; set; }// 租用时长
	}

	/// <summary>
	/// 补充订单
	/// </summary>
	public struct ReturnSupplementaryOrder {
		public ReturnSupplementaryOrder(SupplementaryOrders supplementaryOrders) {
			Id = supplementaryOrders.Id;
			TheOrder = supplementaryOrders.TheOrder;
			OutTradeNo = supplementaryOrders.OutTradeNo;
			Total = supplementaryOrders.Total;
			Status = supplementaryOrders.Status;
			CreatedAt = supplementaryOrders.CreatedAt;
		}

		public int Id { get; init; }
		public int TheOrder { get; set; }
		public string? OutTradeNo { get; set; }

		/// <summary>
		/// 【总金额】订单总金额
		/// </summary>
		public decimal Total { get; set; } = 0;
		/// <summary>
		/// 已付
		/// </summary>
		public decimal Paid { get; set; } = 0;
		/// <summary>
		/// 订单状态
		/// </summary>
		public EOrderStatus Status { get; set; }
		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreatedAt { get; set; }
	}
}
