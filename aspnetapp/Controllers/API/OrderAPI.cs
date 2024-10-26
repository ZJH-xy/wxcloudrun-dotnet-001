namespace aspnetapp.Controllers.API {

    [Route("order")]
    [ApiController]
    public class OrderAPI : ControllerBase {
        private readonly OrderController orderController = new(new MyDbContext());

        // 查询单个订单
        [HttpGet("i/{orderId}")]
        public async Task<IActionResult> GetOderById(int orderId) {
            Order? order;
            try {
                order = await orderController.GetById(orderId);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetOderByUserId: {e}");
#endif
                return StatusCode(500);
            }
            
            if (order == null)
                return StatusCode(404);

            return StatusCode(200, new ReturnOrder(order));
        }

        // 获取用户最近20条订单
        [HttpGet("i/a/{userId}")]
        public async Task<IActionResult> GetOderByUserId(int orderId) {
            List<Order> orderList;
            try {
                orderList = await orderController.GetOrderByUserId(orderId);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetOderByUserId: {e}");
#endif
                return StatusCode(500);
            }

            List<ReturnOrder> returnOrderorderList = new();

            foreach (Order order in orderList)
                returnOrderorderList.Add(new ReturnOrder(order));

            return StatusCode(200, returnOrderorderList);
        }

        // 计算租金
        [HttpGet("calculate")]
        public IActionResult CalculateRent(GetCalculateRent getCalculateRent) {
            decimal cost = 0;
            return StatusCode(200, cost);
        }

        // 创建订单
        [HttpPost("a")]
        public async Task<IActionResult> AddOrder(GetOrder data) {
            // 订单信息合法性验证
            using MyDbContext dbcontext = new();

            if (await dbcontext.User.SingleOrDefaultAsync(u => u.UserId == data.TheUser) is null)
                return StatusCode(403, "用户不存在");

            if (data.UserName == string.Empty)
                return StatusCode(403, "请检查名字格式");

            if (data.IdentityCard == string.Empty)
                return StatusCode(403, "请检查身份证号格式");

            if (data.StartingTime != DateTime.Today)
                return StatusCode(403, "起始时间只能为今日");

            if (data.UserPhone == string.Empty || System.Text.RegularExpressions.Regex.IsMatch(data.UserPhone, @"^1(3[0-9]|4[01456879]|5[0-35-9]|6[2567]|7[0-8]|8[0-9]|9[0-35-9])\d{8}$") == false)
                return StatusCode(403, "请检查手机号码格式");

            // 检查是否有订单待付款
            try {
                if (await dbcontext.Order.Where(o => o.TheUser == data.TheUser).
                    AnyAsync(o => o.Status == Order.OrderStatus.待付款)) {
                    return StatusCode(403, "当前有待付款的订单");
                }
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]AddOrder，获取用户订单信息异常: {e}");
#endif
                return StatusCode(404, "订单信息获取错误");
            }

            Vehicle? vehicle;
            try {
                vehicle = await dbcontext.Vehicle.SingleOrDefaultAsync(v => v.IsDelete == false && v.VehicleId == data.Vehicle);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]AddOrder，获取车辆信息异常: {e}");
#endif
                return StatusCode(505);
            }
            if (vehicle is null)
                return StatusCode(404, "车辆不存在或状态异常");

            if (vehicle.State != Vehicle.Estates.空闲)
                return StatusCode(403, "手慢了，请更换车辆");

            vehicle.State = Vehicle.Estates.锁定;
            vehicle.StateUpdatedAt = DateTime.Now;
            vehicle.UpdatedAt = DateTime.Now;

            dbcontext.Vehicle.Update(vehicle);
            try {
                await dbcontext.SaveChangesAsync();

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]AddOrder，车辆锁定异常: {e}");
#endif
                return StatusCode(500, "车辆锁定失败");
            }

            Order order = new() {
                TheUser = data.TheUser,
                UserName = data.UserName,// 用户姓名
                UserPhone = data.UserPhone,// 用户手机号
                IdentityCard = data.IdentityCard,// 身份证号
                StartingTime = data.StartingTime,// 起始时间
                ExpectedReturnTime = data.ExpectedReturnTime,// 预计归还时间
                RentalLocation = data.RentalLocation,// 租车点
                LongTermLease = data.LongTermLease, // 长租
                Deposit = 0,// 押金
                Rent = 0,// 租金
                Notes = data.Notes,// 备注
                Status = Order.OrderStatus.待付款,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            try {
                int changes = await orderController.AddOrder(order);
                if (0 == changes) {
                    throw new Exception("新增行数为0");
                }
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]AddOrder，创建订单异常: {e},\n" +
                    $"order:{order}");
#endif
                return StatusCode(403, "创建订单失败，请联系管理员");
            }

            // 获取新建的订单id
            Order newOrder;
            try {
                /* 待付款的订单最多只能有一条 */
                newOrder = await dbcontext.Order.SingleAsync(o => o.TheUser == order.TheUser && o.Status == Order.OrderStatus.待付款);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]AddOrder，查询订单结果异常: {e}");
#endif
                return StatusCode(403, "订单状态异常");
            }

            return StatusCode(201, newOrder.OrderId);
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

            return StatusCode(200, "支付成功");
        }

        // 检查未支付订单并取消超过10分钟的订单
        public async Task CheckOrderPayment() {
            using MyDbContext _dbContext = new();
            var now = DateTime.UtcNow; // 获取当前时间
            var threshold = now.AddMinutes(-10); // 计算10分钟前的时间

            // 查询所有超过10分钟未支付的待付款订单
            var ordersToCancel = await _dbContext.Order
                .Where(o => o.Status == Order.OrderStatus.待付款 && o.CreatedAt < threshold)
                .ToListAsync();

            foreach (var order in ordersToCancel) {
                order.Status = Order.OrderStatus.已取消;
                order.UpdatedAt = now; // 更新取消时间
#if DEBUG
                Console.WriteLine($"[日志]CheckOrderPayment 订单取消：order: {order}");
#endif
            }

            try {
                await _dbContext.SaveChangesAsync(); // 保存更改

            } catch (Exception) {
#if DEBUG
                Console.WriteLine($"[错误]CheckOrderPayment，订单状态修改异常");
#endif
            }
        }
    }

    // 计算租用费用
    public class GetCalculateRent {
        public int RentalLocation { get; set; }// 租车点（StoreId）
        public DateTime StartingTime { get; set; }// 起始时间
        public DateTime ExpectedReturnTime { get; set; }// 预计归还时间
    }

    // 获取创建订单信息
    public class GetOrder {
        public int TheUser { get; set; }
        public string UserName { get; set; }// 用户姓名
        public string UserPhone { get; set; }// 用户手机号
        public string IdentityCard { get; set; }// 身份证号
        public int Vehicle { get; set; }// 租用车辆
        public DateTime StartingTime { get; set; }// 起始时间
        public DateTime ExpectedReturnTime { get; set; }// 预计归还时间
        public int RentalLocation { get; set; }// 租车点（StoreId）
        public bool LongTermLease { get; set; }// 长租
        public string Notes { get; set; }// 备注
    }

    // 返回订单格式
    public struct ReturnOrder {
        public ReturnOrder(Order order) {
            OrderId = order.OrderId;
            TheUser = order.TheUser;
            StartingTime = order.StartingTime;
            ExpectedReturnTime = order.ExpectedReturnTime;
            ActualReturnTime = order.ActualReturnTime;
            Vehicle = order.Vehicle;
            RentalLocation = order.RentalLocation;
            ReturnThePoint = order.ReturnThePoint;
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
        public int TheUser { get; set; }
        public DateTime StartingTime { get; set; }// 起始时间
        public DateTime ExpectedReturnTime { get; set; }// 预计归还时间
        public DateTime? ActualReturnTime { get; set; }// 实际归还时间
        public int Vehicle { get; set; }// 租用车辆
        public int RentalLocation { get; set; }// 租车点（StoreId）
        public int? ReturnThePoint { get; set; }// 还车点（StoreId）
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
