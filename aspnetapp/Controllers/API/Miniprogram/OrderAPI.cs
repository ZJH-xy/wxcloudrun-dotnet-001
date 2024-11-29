using aspnetapp.Controllers.Miniprogram;
using Microsoft.AspNetCore.Authorization;
using MySqlConnector;
using System.Security.Claims;
using Senparc.Weixin.TenPayV3.Apis;
using Senparc.Weixin.TenPayV3.Apis.BasePay;
using Senparc.Weixin.Exceptions;
using Senparc.CO2NET.Helpers;
using Senparc.CO2NET.Extensions;

namespace aspnetapp.Controllers.API.Miniprogram
{

    [Route("order")]
    [ApiController]
    [Authorize]// 方法受到限制
    public class OrderAPI : ControllerBase {
        private readonly MyDbContext _dbContext;
        private readonly ILogger<OrderAPI> _logger;
        private readonly OrderController _orderController;

        public OrderAPI(MyDbContext dbContext, ILogger<OrderAPI> logger) {
            _dbContext = dbContext;
            _logger = logger;
            _orderController = new(_dbContext);
        }

        /// <summary>
        /// 计算总租金
        /// </summary>
        /// <param name="rentalLocation">租车门店Id</param>
        /// <param name="menuId">套餐Id</param>
        /// <returns></returns>
        [AllowAnonymous]// 允许匿名访问
        [HttpGet("calculate/{rentalLocation}/{menuId}")]
        public async Task<IActionResult> CalculateRent(int rentalLocation, int menuId) {
            Store? store = await _dbContext.Store.SingleOrDefaultAsync(s => s.Id == rentalLocation && !s.IsDelete);
            if (store is null)
                return StatusCode(404);

            StoreMenu? storeMenus = await _dbContext.StoreMenus.SingleOrDefaultAsync(sm => sm.Id == menuId && sm.TheStore == store.Id && !sm.IsDelete);
            if (storeMenus is null)
                return StatusCode(404);

            return StatusCode(200, storeMenus.Rent + storeMenus.Deposit);
        }

        /// <summary>
        /// 用户查询单个订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("i/{orderId}")]
        public async Task<IActionResult> GetOderById(int orderId) {
            Order? order;
            try {
                order = await _orderController.GetById(GetUserIdInt(), orderId);

            } catch (Exception e) {
                _logger.LogError(e, "用户{UserId}查询订单{order}信息", GetUserIdInt(), orderId);

                return StatusCode(500);
            }

            if (order == null)
                return StatusCode(404);

            return StatusCode(200, new ReturnOrder(order));
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

            List<ReturnOrderBasic> returnOrderorderList = new();

            foreach (Order order in orderList)
                returnOrderorderList.Add(new ReturnOrderBasic(order));

            return StatusCode(200, returnOrderorderList);
        }

        /// <summary>
        /// 获取订单状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("status/i/{orderId}")]
        public async Task<IActionResult> GetOderStatusByUserId(int orderId) {
            Order.OrderStatus? orderStatus;
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
                    AnyAsync(o => o.Status == Order.OrderStatus.待付款)) {
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

            // 锁定车辆，更新时间
            vehicle.State = Vehicle.Estates.锁定;
            vehicle.StateUpdatedAt = DateTime.Now;
            vehicle.UpdatedAt = DateTime.Now;

            try {
                await _dbContext.SaveChangesAsync();// 保存车辆状态

            } catch (Exception e) {
                _logger.LogError(e, "车辆{Vehicle}锁定，操作用户{UserId}", vehicle.Id, userId);

                return StatusCode(500, "车辆锁定失败");
            }

            Order order = new() {
                TheUser = userId,
                TheRentalLocation = store.Id,
                TheVehicle = data.Vehicle,// 车辆
                TheStoreMenu = data.StoreMenuId,// 套餐Id
                UserName = data.UserName,// 用户姓名
                UserPhone = data.UserPhone,// 用户手机号
                IdentityCard = data.IdentityCard,// 身份证号
                Deposit = 0,// 押金
                Rent = 0,// 租金
                Status = Order.OrderStatus.待付款,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _logger.LogDebug("订单创建信息{OrderId}", order.Id);

            try {
                int changes = await _orderController.AddOrder(order);
                if (0 == changes) {
                    throw new Exception("新增行数为0");
                }

                _logger.LogInformation("订单{OrderId}创建", order.Id);

            } catch (Exception e) {
                _logger.LogError(e, "创建订单{OrderId}", order.Id);

                return StatusCode(403, "创建订单失败，请联系管理员");
            }

            // 获取新建订单的id
            return StatusCode(201, order.Id);
        }
		#endregion

		#region 支付
		/// <summary>
		/// 支付接口，支付成功后更改订单状态
		/// </summary>
		/// <param name="orderId"></param>
		/// <returns></returns>
		[HttpPost("pay")]
        public async Task<IActionResult> PayOrder(PayData data) {
			Order? order = await _dbContext.Order.SingleOrDefaultAsync(o => o.Id == data.orderId);
            //Order? order = await _orderController.GetById(GetUserIdInt(), data.orderId);


            if (order is null || order.Status != Order.OrderStatus.待付款) {
                return StatusCode(403, "订单不存在或无法支付");
            }

            // 订单付款中
            //order.Status = Order.OrderStatus.付款中;
            //order.UpdatedAt = DateTime.Now;
            //try {
            //    _dbContext.Order.Update(order);
            //    await _dbContext.SaveChangesAsync();

            //} catch (Exception e) {
            //    _logger.LogError(e, "更改订单{OrderId}状态为付款中", order.Id);
            //    return StatusCode(500);
            //}

            #region 微信支付小程序下单流程
            //
            string appid = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
            string mchid = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId;
			// 商品描述
			string description = "测试";
			// 商户订单号
			string outTradeNo = data.orderId.ToString();
			// 交易结束时间
			string time_expire = DateTime.Now.AddMinutes(120).ToString("yyyy-MM-ddTHH:mm:sszzz");
			// 附加数据
			string attach = "";
			// 订单总金额
			int total = 1;			

			BasePayApis basePayApis = new BasePayApis();

            TransactionsRequestData requestData = new TransactionsRequestData {
                appid = appid,

                // 【直连商户号】 直连商户号
                mchid = mchid,

                // 【商品描述】 商品描述
                description = description,

                // 【商户订单号】 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一。
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
                notify_url = "https://wxcloudrun-dotnet-128645-8-1331625129.sh.run.tcloudbase.com/order/notify",

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
                    openid = data.openid
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
            
            // 创建订单
			JsApiReturnJson result = await basePayApis.JsApiAsync(requestData);

			// 【预支付交易会话标识】 预支付交易会话标识。用于后续接口调用中使用，该值有效期为2小时
			string prepayId = result.prepay_id;

            if (prepayId.IsNullOrEmpty()) {
                _logger.LogError("[PayOrder]创建订单错误");
                return StatusCode(500);
            }

			// 加密
			//string appid = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
			long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			string nonceStr = Guid.NewGuid().ToString("N");
			string pack = "prepay_id=" + prepayId;
			string signType = "RSA";
			String paySign = GetSign(appid, timestamp, nonceStr, pack);//签名


			if (result.VerifySignSuccess != true) {
                _logger.LogError("获取 prepay_id 结果校验出错！");
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

		// https://www.cnblogs.com/dawenyang/p/14458773.html
		public async Task<WxPayCallbackViewModel> WxPayCallback1() {
			var buffer = new MemoryStream();
			Request.Body.CopyTo(buffer);
			var str = Encoding.UTF8.GetString(buffer.GetBuffer());


			var wxPayNotifyModel = str.GetObject<WxPayNotifyModel>();


			var resource = wxPayNotifyModel?.resource ?? new WxPayResourceModel();
			var decryptStr = aspnetapp.Controllers.API.Background.AesGcm.AesGcmDecrypt(resource.associated_data, resource.nonce, resource.ciphertext);
			var decryptModel = decryptStr.GetObject<WxPayResourceDecryptModel>();

			//var viewModel = new WxPayCallbackViewModel();

            // 发起查询订单请求
			//if (decryptModel is not null) {
			//	var url = $"https://api.mch.weixin.qq.com/v3/pay/transactions/out-trade-no/{decryptModel.out_trade_no}?mchid={WxPayConst.mchid}";
			//	var client = new HttpClient(new WxPayHelper());
			//	var resp = await client.GetAsync(url);
			//	var respStr = await resp.Content.ReadAsStringAsync();
			//	var payModel = respStr.GetObject<WxPayStatusRespModel>();

			//	return viewModel;
			//}

			//viewModel.code = "FAIL";
			//viewModel.message = "数据解密失败";
			return new WxPayCallbackViewModel();
		}
		#endregion

		#region 支付回调
		/// <summary>
		/// 支付回调
		/// </summary>
		/// <returns></returns>
		[AllowAnonymous]// 允许匿名访问
		[HttpPost("notify")]
		public async Task<IActionResult> WxPayCallback(WxPayNotifyModel data) {
            //Senparc.Weixin.TenPayV3.Apis.BasePay.Entities.RefundNotifyJson//本类型为微信支付回调通知退款信息
#if DEBUG
            _logger.LogDebug("收到微信支付回调：{data}", data.resource);
#endif

            // 解密
            var decryptStr = aspnetapp.Controllers.API.Background.AesGcm.AesGcmDecrypt(data.resource.associated_data, data.resource.nonce, data.resource.ciphertext);
			var decryptModel = decryptStr.GetObject<WxPayResourceDecryptModel>();


            if (decryptModel.trade_state != "SUCCESS") {
                // 订单支付未成功
                _logger.LogInformation("订单支付状态{trade_state}", decryptModel.trade_state);

            }

			// 更新订单已付
			//order.Status = Order.OrderStatus.待确认;
			//order.UpdatedAt = DateTime.Now;
			//try {
			//	_dbContext.Order.Update(order);
			//	await _dbContext.SaveChangesAsync();

			//} catch (Exception e) {
			//	_logger.LogCritical(e, "保存订单信息{OrderId}", order.Id);
			//	return StatusCode(500);
			//}

			//_logger.LogInformation("用户{UserId}支付订单{OrderId}成功", GetUserIdInt(), data.orderId);



			// 模拟

			if (false) {
				// 接收失败： HTTP应答状态码需返回5XX或4XX，同时需返回应答报文
				WxPayCallbackViewModel wxPayCallbackViewModel = new WxPayCallbackViewModel {
					code = "FAIL",
					message = "失败"
				};

				return StatusCode(500, wxPayCallbackViewModel);
			}

			// 处理完成
			return StatusCode(200);
		}
		#endregion

		/// <summary>
		/// 换车请求
		/// </summary>
		/// <param name="getData"></param>
		/// <returns></returns>
		[HttpPost("replacement")]
        public async Task<IActionResult> Replacement(GetReplacementInfo getData) {
            // 检查订单状态
            Order? order = await _orderController.GetById(GetUserIdInt(), getData.OrderId);
            if (order is null || order.Status != Order.OrderStatus.进行中) {
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

            using var transaction = await _dbContext.Database.BeginTransactionAsync();// 事务开始

            // 锁定车辆
            vehicle.State = Vehicle.Estates.锁定;

            // 添加至换车表
            VehicleReplacementRecord vrr = new() {
                TheOrder = getData.OrderId,// 订单Id
                TheOldVehicles = order.TheVehicle,// 旧车辆
                TheNewVehicles = getData.ReplacementVehicleId,// 要更换的车辆
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

        /// <summary>
        /// 取消换车请求
        /// </summary>
        /// <param name="getReplacementVehicle"></param>
        /// <returns></returns>
        [HttpPost("replacement/cancel")]
        public async Task<IActionResult> CancelReplacement(GetCancelReplacementInfo getData) {
            // 检查订单状态
            Order? order = await _orderController.GetById(GetUserIdInt(), getData.OrderId);

            if (order is null || order.Status != Order.OrderStatus.进行中) {
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
                } catch (DbUpdateConcurrencyException ex) {
                    _logger.LogWarning(ex, "并发冲突，无法更新订单{OrderId}或车辆{VehicleId}状态", v.Id, v.TheNewVehicles);

                    // 可选择：重新查询、通知用户或跳过冲突条目
                } catch (Exception ex) {
                    _logger.LogCritical(ex, "更新订单{OrderId}或车辆{VehicleId}状态失败", v.Id, v.TheNewVehicles);

                    await transaction.RollbackAsync();
                    throw;
                }
            }

            await transaction.CommitAsync();
        }

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
                    .Where(o => o.Status == Order.OrderStatus.待付款 && o.CreatedAt < threshold)
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
                    order.Status = Order.OrderStatus.已取消;
                    order.UpdatedAt = now;

                    Vehicle vehicle = await _dbContext.Vehicle
                        .SingleAsync(v => v.Id == order.TheVehicle);

                    vehicle.State = Vehicle.Estates.空闲;
                    vehicle.UpdatedAt = now;

                    _logger.LogInformation("订单{OrderId}取消, 车辆{VehicleId}状态更新为空闲", order.Id, vehicle.Id);

                    await _dbContext.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException ex) {
                    _logger.LogWarning("并发冲突，订单或车辆记录已被更新：{OrderId}, {Exception}", order.Id, ex);

                } catch (Exception ex) {
                    _logger.LogCritical(ex, "订单或车辆状态事务失败，ordersToCancel：{OrdersToCancel}",
                        ordersToCancel.Select(o => o.Id).ToArray());

                    await transaction.RollbackAsync();
                    throw;
                }
            }

            await transaction.CommitAsync();
            return;
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
		public string GetSign(string appId, long timestamp, string nonceStr, string pack) {
			string message = BuildMessage(appId, timestamp, nonceStr, pack);
			string paySign = Sign(message);
			return paySign;
		}

		// 构建消息
		private string BuildMessage(string appId, long timestamp, string nonceStr, string pack) {
			return $"{appId}\n{timestamp}\n{nonceStr}\n{pack}\n";
		}

		// 签名方法
		private string Sign(string message) {
			// 获取商户私钥明文
			string privateKey = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_PrivateKey;

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
	}

	/// <summary>
	/// 下单用
	/// </summary>
	public class PayData {
		public string openid { get; set; }

        public int orderId { get; set; }
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
	/// 微信支付回调通知结果解密实体
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
		/// 支付者信息
		/// </summary>
		public WxPayerResourceDecryptModel payer { set; get; }
	}
	/// <summary>
	/// 支付用户信息实体
	/// </summary>
	public class WxPayerResourceDecryptModel {
		/// <summary>
		/// 用户在直连商户appid下的唯一标识。
		/// </summary>
		public string openid { get; set; }
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
    /// 基础返回订单
    /// </summary>
    public struct ReturnOrderBasic {
        public ReturnOrderBasic(Order order) {
            OrderId = order.Id;
            TheRentalLocation = order.TheRentalLocation;
            TheVehicle = order.TheVehicle;
            UserName = order.UserName;
            UserPhone = order.UserPhone;
            Deposit = order.Deposit;
            Rent = order.Rent;
            DispatchFee = order.DispatchFee;
            OtherFees = order.OtherFees;
            Paid = order.Paid;
            DepositRefunded = order.DepositRefunded;
            Status = order.Status;
            CreatedAt = order.CreatedAt;
        }
        public int OrderId { get; init; }// 订单编号
        public int TheRentalLocation { get; set; }// 租车点（StoreId）
        public int TheVehicle { get; set; }// 租用车辆
        public string UserName { get; set; }// 用户姓名
        public string UserPhone { get; set; }// 用户手机号
        public decimal Deposit { get; set; }// 押金
        public decimal Rent { get; set; }// 租金
        public decimal DispatchFee { get; set; }// 调度费
        public decimal OtherFees { get; set; }// 其他费用
        public decimal Paid { get; set; }// 已付
        public decimal DepositRefunded { get; set; }// 已退押金
        public Order.OrderStatus Status { get; set; }// 订单状态
        public DateTime CreatedAt { get; set; }
    }

    /// 详细返回订单格式
    public struct ReturnOrder {
        public ReturnOrder(Order order) {
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
            OtherFees = order.OtherFees;
            Paid = order.Paid;
            DepositRefunded = order.DepositRefunded;
            Status = order.Status;
            Notes = order.Notes;
            CreatedAt = order.CreatedAt;
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
        public decimal OtherFees { get; set; }// 其他费用
        public decimal Paid { get; set; }// 已付
        public decimal DepositRefunded { get; set; }// 已退押金
        public Order.OrderStatus Status { get; set; }// 订单状态
        public string? Notes { get; set; }// 备注
        public DateTime CreatedAt { get; set; }
    }
}
