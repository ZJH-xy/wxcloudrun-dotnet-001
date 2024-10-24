using System.Threading.Channels;

namespace aspnetapp.Controllers.API {

    [Route("order")]
    [ApiController]
    public class OrderAPI : ControllerBase {
        readonly OrderController orderController = new(new MyDbContext());

        // 查询单个订单
        [HttpGet("i/{id}")]
        public async Task<IActionResult> GetOderByUserId(int id) {
            Order? order = null;
            try {
                order = await orderController.GetById(id);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]GetOderByUserId: {e}");
#endif
                return StatusCode(500);
            }

            return StatusCode(200, order);
        }

        // 计算租金
        [HttpGet("calculate")]
        public async Task<IActionResult> CalculateRent(GetCalculateRent getCalculateRent) {
            return StatusCode(404);
            //decimal sum = 0;
        }

        // 创建订单
        [HttpPost("a")]
        public async Task<IActionResult> AddOrder(GetOrder data) {
            using MyDbContext dbcontext = new();
            try {
                if (await dbcontext.Order.Where(o => o.TheUser == data.TheUser).
                Where(o => o.Status == Order.OrderStatus.待确认 || o.Status == Order.OrderStatus.待付款).AnyAsync()) {
                    return StatusCode(403, "当前有待确认的订单");
                }
            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]AddOrder，获取用户订单信息异常: {e}");
#endif
                return StatusCode(403, "订单信息获取错误");
            }

            Vehicle? vehicle = null;
            try {
                vehicle = await dbcontext.Vehicle.SingleOrDefaultAsync(v => v.IsDelete == false && v.VehicleId == data.Vehicle);

            } catch (Exception e) {
#if DEBUG
                Console.WriteLine($"[错误]AddOrder，获取车辆信息异常: {e}");
#endif
                return StatusCode(403, "获取车辆信息错误");
            }
            if (vehicle is null)
                return StatusCode(403, "车辆选择错误");

            if (vehicle.State != Vehicle.Estates.空闲)
                return StatusCode(403, "手慢了，请更换车辆");

            vehicle.State = Vehicle.Estates.锁定;
            vehicle.StateUpdatedAt = DateTime.Now;
            vehicle.UpdatedAt = DateTime.Now;

            try {
                dbcontext.Vehicle.Update(vehicle);
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
                Deposit = data.Deposit,// 押金
                Rent = data.Rent,// 租金
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
                Console.WriteLine($"[错误]AddOrder，创建订单异常: {e}");
#endif
                return StatusCode(403, "创建订单失败");
            }

            return StatusCode(201, new ReturnOrder(order));
        }
    }

    public class GetCalculateRent {
        public bool LongTermLease { get; set; }// 长租
        public decimal Deposit { get; set; }// 押金
        public decimal Rent { get; set; }// 租金
    }

    // 创建订单类
    public class GetOrder {
        public int TheUser { get; set; }
        public string UserName { get; set; }// 用户姓名
        public string UserPhone { get; set; }// 用户手机号
        public string? IdentityCard { get; set; }// 身份证号
        public int Vehicle { get; set; }// 租用车辆
        public DateTime StartingTime { get; set; }// 起始时间
        public DateTime ExpectedReturnTime { get; set; }// 预计归还时间
        public int RentalLocation { get; set; }// 租车点（StoreId）
        public bool LongTermLease { get; set; }// 长租
        public decimal Deposit { get; set; }// 押金
        public decimal Rent { get; set; }// 租金
        public string? Notes { get; set; }// 备注
    }

    public class ReturnOrder {
        public ReturnOrder(Order order) {
            Vehicle = order.Vehicle;
        }
        public int Vehicle { get; set; }
    }
}
