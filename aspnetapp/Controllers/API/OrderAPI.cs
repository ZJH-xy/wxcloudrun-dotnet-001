using aspnetapp.Models;
using Microsoft.AspNetCore.Authorization;
using Polly;
using Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp.WxAppJson;
using System.Security.Claims;

namespace aspnetapp.Controllers.API {

    [Route("order")]
    [ApiController]
    [Authorize]// 方法受到限制
    public class OrderAPI : ControllerBase {
        private readonly OrderController orderController = new(new MyDbContext());
        private readonly ILogger<OrderAPI> _logger;

        public OrderAPI(ILogger<OrderAPI> logger) {
            _logger = logger;
        }

        // 计算租金
        [AllowAnonymous]// 允许匿名访问
        [HttpGet("calculate")]
        public async Task<IActionResult> CalculateRent(GetCalculateRent getCalculateRent) {
            using MyDbContext dbcontext = new();

            Store? store = await dbcontext.Store.SingleOrDefaultAsync(s => s.Id == getCalculateRent.RentalLocation && !s.IsDelete);
            if (store is null)
                return StatusCode(404);

            StoreMenu? storeMenus = await dbcontext.StoreMenus.SingleOrDefaultAsync(sm => sm.Id == getCalculateRent.MenuId && sm.TheStore == store.Id && !sm.IsDelete);
            if (storeMenus is null)
                return StatusCode(404);

            return StatusCode(200, storeMenus.Rent + storeMenus.Deposit);
        }

        // 用户查询单个订单
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
        
        // 获取用户最近10条订单
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

        // 创建订单
        [HttpPost("add")]
        public async Task<IActionResult> AddOrder(GetOrder data) {
            using MyDbContext dbcontext = new();
            // 订单信息合法性验证

            if (await dbcontext.User.SingleOrDefaultAsync(u => u.Id == GetUserIdInt()) is null)
                return StatusCode(403, "用户不存在");

            if (!data.DepositRequired) {
                if (!Regex.IsMatch(data.IdentityCard, @"^(^\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$", RegexOptions.IgnoreCase))
                    return StatusCode(403, "请检查身份证号格式");
            }

            if (data.UserName == string.Empty)
                return StatusCode(403, "请检查名字格式");

            if (!Regex.IsMatch(data.UserPhone, @"^1(3[0-9]|4[01456879]|5[0-35-9]|6[2567]|7[0-8]|8[0-9]|9[0-35-9])\d{8}$"))
                return StatusCode(403, "请检查手机号码格式");

            // 检查用户订单状态
            try {
                if (await dbcontext.Order.Where(o => o.TheUser == GetUserIdInt()).
                    AnyAsync(o => o.Status == Order.OrderStatus.待付款)) {
                    return StatusCode(403, "当前有待付款的订单");
                }
            } catch (Exception e) {
                _logger.LogError(e, "查询用户{UserId}未完成的订单信息", GetUserIdInt());

                return StatusCode(404, "订单信息获取错误");
            }

            // 检查门店状态
            Store? store;
            var storeMenus = await dbcontext.StoreMenus.SingleOrDefaultAsync(sm => sm.Id == data.StoreMenuId && !sm.IsDelete);
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

            dbcontext.Vehicle.Update(vehicle);
            try {
                await dbcontext.SaveChangesAsync();

            } catch (Exception e) {
                _logger.LogError(e, "车辆{Vehicle}锁定", vehicle.Id);

                return StatusCode(500, "车辆锁定失败");
            }

            Order order = new() {
                TheUser = GetUserIdInt(),
                TheVehicle = data.Vehicle,// 车辆
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

        // 支付接口，支付成功后取消自动取消计时器
        [HttpPost("pay/{orderId}")]
        public async Task<IActionResult> PayOrder(int orderId) {
            using MyDbContext dbContext = new();
            Order? order = await dbContext.Order.FindAsync(orderId);

            if (order == null || order.Status != Order.OrderStatus.待付款) {
                return StatusCode(404, "订单不存在或无法支付");
            }

            order.Status = Order.OrderStatus.待确认;
            order.UpdatedAt = DateTime.Now;
            await dbContext.SaveChangesAsync();

            // 取消自动取消计时任务

            _logger.LogInformation("用户{UserId}订单{OrderId}支付成功", GetUserIdInt(), orderId);

            return StatusCode(200, "支付成功");
        }

        // 检查未支付订单并取消超过10分钟的订单
        public async Task CheckOrderPayment() {
            using MyDbContext _dbContext = new();
            DateTime now = DateTime.Now;// 获取当前时间
            DateTime threshold = now.AddMinutes(-10);// 计算10分钟前的时间

            // 查询所有超过10分钟未支付的待付款订单
            List<Order> ordersToCancel = await _dbContext.Order
                .Where(o => o.Status == Order.OrderStatus.待付款 && o.CreatedAt < threshold)
                .ToListAsync();

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            foreach (var order in ordersToCancel) {
                // 更新订单状态
                order.Status = Order.OrderStatus.已取消;
                order.UpdatedAt = now;

                // 查询并更新与订单关联的车辆状态为空闲
                Vehicle vehicle = await _dbContext.Vehicle
                    .SingleAsync(v => v.Id == order.TheVehicle);
                    
                vehicle.State = Vehicle.Estates.空闲;
                vehicle.UpdatedAt = now;
                    
                _logger.LogInformation("订单{OrderId}取消, 车辆{VehicleId}状态更新为空闲", order.Id, vehicle.Id);
            }

            try {
                await _dbContext.SaveChangesAsync();// 保存所有更改
                await transaction.CommitAsync();// 提交事务

            } catch (Exception e) {
                _logger.LogCritical(e, "订单或车辆状态更新失败，ordersToCancel{OrdersToCancel}",
                    ordersToCancel.Select(o => o.Id).ToArray());

                await transaction.RollbackAsync();// 回滚事务
            }
        }

        /// <summary>
        /// JWT 获取用户id
        /// </summary>
        /// <returns></returns>
        public int GetUserIdInt() {
            return int.Parse(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }


    // 计算租用费用获取
    public class GetCalculateRent {
        /// <summary>
        /// 租车点（StoreId）
        /// </summary>
        public int RentalLocation { get; set; }
        /// <summary>
        /// 套餐Id
        /// </summary>
        public int MenuId { get; set; }
    }

    // 获取创建订单信息
    public class GetOrder {
        public int StoreMenuId { get; set; }// 套餐Id
        public bool DepositRequired { get; set; }// 需要押金
        public string UserName { get; set; }// 用户姓名
        public string UserPhone { get; set; }// 用户手机号
        public string IdentityCard { get; set; }// 身份证号
        public int Vehicle { get; set; }// 租用车辆
    }

    // 基础返回订单
    public struct ReturnOrderBasic {
        public ReturnOrderBasic(Order order) {
            OrderId = order.Id;
            TheVehicle = order.TheVehicle;
            UserName = order.UserName;
            UserPhone = order.UserPhone;
            LongTermLease = order.LongTermLease;
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
        public int TheVehicle { get; set; }// 租用车辆
        public string UserName { get; set; }// 用户姓名
        public string UserPhone { get; set; }// 用户手机号
        public bool LongTermLease { get; set; }// 长租
        public decimal Deposit { get; set; }// 押金
        public decimal Rent { get; set; }// 租金
        public decimal DispatchFee { get; set; }// 调度费
        public decimal OtherFees { get; set; }// 其他费用
        public decimal Paid { get; set; }// 已付
        public decimal DepositRefunded { get; set; }// 已退押金
        public Order.OrderStatus Status { get; set; }// 订单状态
        public DateTime CreatedAt { get; set; }
    }

    // 详细返回订单格式
    public struct ReturnOrder {
        public ReturnOrder(Order order) {
            OrderId = order.Id;
            ActualStartingTime = order.ActualStartingTime;
            ActualReturnTime = order.ActualReturnTime;
            TheVehicle = order.TheVehicle;
            TheRentalLocation = order.TheRentalLocation;
            TheReturnThePoint = order.TheReturnThePoint;
            UserName = order.UserName;
            UserPhone = order.UserPhone;
            LongTermLease = order.LongTermLease;
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
        public int TheVehicle { get; set; }// 租用车辆
        public int TheRentalLocation { get; set; }// 租车点（StoreId）
        public int? TheReturnThePoint { get; set; }// 还车点（StoreId）
        public string UserName { get; set; }// 用户姓名
        public string UserPhone { get; set; }// 用户手机号
        public bool LongTermLease { get; set; } = false;// 长租
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
