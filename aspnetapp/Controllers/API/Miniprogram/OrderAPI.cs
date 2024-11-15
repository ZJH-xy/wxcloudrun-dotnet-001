using aspnetapp.Controllers.Miniprogram;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace aspnetapp.Controllers.API.Miniprogram
{

    [Route("order")]
    [ApiController]
    [Authorize]// 方法受到限制
    public class OrderAPI : ControllerBase {

        private readonly MyDbContext _dbContext;
        private readonly ILogger<OrderAPI> _logger;
        private readonly OrderController orderController = new(new MyDbContext());

        public OrderAPI(MyDbContext dbContext, ILogger<OrderAPI> logger) {
            _dbContext = dbContext;
            _logger = logger;
            orderController = new(dbContext);
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
            using MyDbContext dbcontext = new();

            Store? store = await dbcontext.Store.SingleOrDefaultAsync(s => s.Id == rentalLocation && !s.IsDelete);
            if (store is null)
                return StatusCode(404);

            StoreMenu? storeMenus = await dbcontext.StoreMenus.SingleOrDefaultAsync(sm => sm.Id == menuId && sm.TheStore == store.Id && !sm.IsDelete);
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
                order = await orderController.GetById(GetUserIdInt(), orderId);

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
                orderList = await orderController.GetOrderByUserId(GetUserIdInt());

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
                orderStatus = await orderController.GetOrderStatusById(GetUserIdInt(), orderId);

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
                status = await orderController.GetOderReplacementByUserId(GetUserIdInt(), orderId);

            } catch (Exception e) {
                _logger.LogError(e, "用户{UserId}查询订单{order}换车状态", GetUserIdInt(), orderId);
                return StatusCode(500);
            }

            return StatusCode(200, status);
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddOrder(GetOrder data) {
            using MyDbContext dbcontext = new();
            int userId = GetUserIdInt();
            // 订单信息合法性验证

            if (await dbcontext.User.SingleOrDefaultAsync(u => u.Id == userId) is null)
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
                if (await dbcontext.Order.Where(o => o.TheUser == userId).
                    AnyAsync(o => o.Status == Order.OrderStatus.待付款)) {
                    return StatusCode(403, "当前有待付款的订单");
                }
            } catch (Exception e) {
                _logger.LogError(e, "查询用户{UserId}未完成的订单信息", userId);

                return StatusCode(404, "订单信息获取错误");
            }

            // 检查门店状态
            Store? store;
            StoreMenu? storeMenus = await dbcontext.StoreMenus.SingleOrDefaultAsync(sm => sm.Id == data.StoreMenuId && !sm.IsDelete);
            if (storeMenus == null)
                return StatusCode(403, "请检查套餐信息");

            store = await dbcontext.Store.SingleOrDefaultAsync(s => s.IsDelete == false && s.Id == storeMenus.TheStore);

            if (store == null)
                return StatusCode(404);

            if (!(store.BusinessStatus && store.IsOpen()))
                return StatusCode(403, "门店未营业");

            // 检查车辆状态
            Vehicle? vehicle;
            try {
                vehicle = await dbcontext.Vehicle.SingleOrDefaultAsync(v => v.IsDelete == false && v.Id == data.Vehicle);

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
                await dbcontext.SaveChangesAsync();// 保存车辆状态

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

            _logger.LogDebug("订单创建信息{Order}", order.ToJson());

            try {
                int changes = await orderController.AddOrder(order);
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

        /// <summary>
        /// 支付接口，支付成功后更改订单状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpPost("pay/{orderId}")]
        public async Task<IActionResult> PayOrder(int orderId) {
            using MyDbContext dbContext = new();
            //Order? order = await dbContext.Order.SingleOrDefaultAsync(o => o.Id == orderId);
            Order? order = await orderController.GetById(GetUserIdInt(), orderId);

            if (order is null || order.Status != Order.OrderStatus.待付款) {
                return StatusCode(403, "订单不存在或无法支付");
            }

            // 订单付款中
            order.Status = Order.OrderStatus.付款中;
            order.UpdatedAt = DateTime.Now;
            try {
                dbContext.Order.Update(order);
                await dbContext.SaveChangesAsync();

            } catch (Exception e) {
                _logger.LogError(e, "更改订单{OrderId}状态为付款中", order.Id);
                return StatusCode(500);
            }

            /* 微信支付流程 */

            // 更新订单已付
            order.Status = Order.OrderStatus.待确认;
            order.UpdatedAt = DateTime.Now;
            try {
                dbContext.Order.Update(order);
                await dbContext.SaveChangesAsync();

            } catch (Exception e) {
                _logger.LogCritical(e, "保存订单信息{OrderId}", order.Id);
                return StatusCode(500);
            }

            _logger.LogInformation("用户{UserId}支付订单{OrderId}成功", GetUserIdInt(), orderId);

            return StatusCode(200, "支付成功");
        }

        /// <summary>
        /// 换车请求
        /// </summary>
        /// <param name="getData"></param>
        /// <returns></returns>
        [HttpPost("replacement")]
        public async Task<IActionResult> Replacement(GetReplacementInfo getData) {
            // 检查订单状态
            Order? order = await orderController.GetById(GetUserIdInt(), getData.OrderId);
            if (order is null || order.Status != Order.OrderStatus.进行中) {
                _logger.LogDebug("订单{OrderId}状态非法", getData.OrderId);
                return StatusCode(403, "订单不存在或非法");
            }

            // 检查是否存在换车请求
            if (await orderController.GetOderReplacementByUserId(GetUserIdInt(), getData.OrderId))
                return StatusCode(403, "当前有侍确认的换车请求");

            // 检查车辆状态
            using MyDbContext dbContext = new();
            Vehicle? vehicle = await dbContext.Vehicle.SingleOrDefaultAsync(v => v.Id == getData.ReplacementVehicleId);
            if (vehicle is null || vehicle.State != Vehicle.Estates.空闲)
                return StatusCode(403, "车辆不存在或状态非法");

            using var transaction = await dbContext.Database.BeginTransactionAsync();// 事务开始

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
                await dbContext.VehicleReplacementRecord.AddAsync(vrr);
                await dbContext.SaveChangesAsync();
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
            Order? order = await orderController.GetById(GetUserIdInt(), getData.OrderId);

            if (order is null || order.Status != Order.OrderStatus.进行中) {
                _logger.LogDebug("订单{OrderId}状态非法", getData.OrderId);
                return StatusCode(403, "订单不存在或非法");
            }

            using MyDbContext dbContext = new();

            // 检查是否存在换车请求
            if (await orderController.GetOderReplacementByUserId(GetUserIdInt(), getData.OrderId))
                return StatusCode(403, "当前有侍确认的换车请求");

            VehicleReplacementRecord? vrr = await dbContext.VehicleReplacementRecord.FirstOrDefaultAsync(vrr => vrr.TheOrder == getData.OrderId && vrr.State == VehicleReplacementRecord.Estates.侍确认);
            if (vrr is null)
                return StatusCode(403, "请求异常");

            vrr.State = VehicleReplacementRecord.Estates.已取消;

            try {
                await dbContext.SaveChangesAsync();

            } catch (Exception e) {
                _logger.LogCritical(e, "取消换车请求{VehicleReplacementRecordId}", vrr.Id);
                return StatusCode(500);
            }

            return StatusCode(200);
        }

        /// <summary>
        /// 检查换车请求，超时取消（暂未使用）
        /// </summary>
        /// <returns></returns>
        public async Task CheckReplacementVehicle() {
            using MyDbContext _dbContext = new();
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
            List<Order> ordersToCancel = await _dbContext.Order
                .Where(o => o.Status == Order.OrderStatus.待付款 && o.CreatedAt < threshold)
                .ToListAsync();

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
        }


        /// <summary>
        /// JWT 获取用户id
        /// </summary>
        /// <returns></returns>
        public int GetUserIdInt() {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
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
