using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using aspnetapp.Models;

namespace aspnetapp.Controllers.API.StoreAccount {

    [Route("storeAccount")]
    [ApiController]
    [Authorize(Roles = "store")]// 只有商家能访问
    public class StoreAccountAPI : ControllerBase {
        private readonly StoreAccountController storeAccountController = new(new MyDbContext());
        //private readonly MyDbContext _dbcontext = new();
        private readonly IOptionsSnapshot<JWTSettings> _JWTSettingsOpt;
        private readonly ILogger<StoreAccountAPI> _logger;

        public StoreAccountAPI(IOptionsSnapshot<JWTSettings> jWTSettingsOpt, ILogger<StoreAccountAPI> logger) {
            _JWTSettingsOpt = jWTSettingsOpt;
            _logger = logger;
        }

        [AllowAnonymous]// 允许匿名访问
        [HttpGet("test")]
        public IActionResult Test() {
            return StatusCode(200);
        }

        // 商家登录
        [AllowAnonymous]// 允许匿名访问
        [HttpGet("login/{account}/{password}")]
        public async Task<IActionResult> Login(string account, string password) {
            using MyDbContext dbcontext = new();
            if (password is null)
                return StatusCode(403, "密码为空");

            Models.StoreAccount? storeAccount = await dbcontext.StoreAccount.SingleOrDefaultAsync(sa => sa.Account == account && sa.Password == password);
            if (storeAccount is null)
                return StatusCode(403, "帐号或密码错误");

            if (await dbcontext.Store.AnyAsync(s => s.Id == storeAccount.Id && s.IsDelete))// 门店是否删除
                return StatusCode(403, "门店已关闭");

            return StatusCode(200, GetJwtToken(CreateClaim(storeAccount.Id.ToString(), "store")));
        }

        // 商家获取订单
        [HttpGet("confirm/{orderId}")]
        public async Task<IActionResult> GetConfirmOder(int orderId) {
            using MyDbContext dbcontext = new();

            Order? order = await storeAccountController.GetConfirmOder(orderId);
            if (order is null)
                return StatusCode(404);// 订单不存在

            if (order.Status != Order.OrderStatus.待确认)
                return StatusCode(403, "订单状态异常");

            return StatusCode(200, new ConfirmReturnOrder(order));
        }

        // 商家确认订单
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmAnOrder(GetConfirmOder getConfirmOder) {
            using MyDbContext dbcontext = new();

            // 检查车辆状态
            Vehicle? vehicle = await dbcontext.Vehicle.FindAsync(getConfirmOder.TheVehicle);

            if (vehicle is null)
                return StatusCode(404);

            // 车辆是否属于当前商家
            int storeId = await dbcontext.StoreAccount.Where(sa => sa.Id == GetUserIdInt()).Select(sa => sa.TheStore).FirstAsync();
            if (vehicle.TheCurrentStore != storeId)
                return StatusCode(403, "请检查更换的车辆");

            if (vehicle.State != Vehicle.Estates.锁定)
                return StatusCode(403, "车辆状态异常");

            // 检查订单状态
            Order? order = await dbcontext.Order.FindAsync(getConfirmOder.OderId);

            if (order is null)
                return StatusCode(404);

            if (order.Status != Order.OrderStatus.待确认)
                return StatusCode(403, "订单状态异常");

            // 确认订单
            order.Status = Order.OrderStatus.进行中;
            try {
                dbcontext.SaveChanges();

            } catch (Exception e) {
                _logger.LogError(e, "订单状态更改");

            }

            return StatusCode(200);
        }

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
    /// 确认订单格式
    /// </summary>
    public class GetConfirmOder {
        public int OderId { get; set; }
        public int TheVehicle { get; set; }// 租用车辆
    }

    // 确认订单返回订单格式
    public struct ConfirmReturnOrder {
        public ConfirmReturnOrder(Order order) {
            OrderId = order.Id;
            ActualStartingTime = order.ActualStartingTime;
            ActualReturnTime = order.ActualReturnTime;
            TheVehicle = order.TheVehicle;
            TheRentalLocation = order.TheRentalLocation;
            UserName = order.UserName;
            UserPhone = order.UserPhone;
            LongTermLease = order.LongTermLease;
            Deposit = order.Deposit;
            Rent = order.Rent;
            DispatchFee = order.DispatchFee;
            OtherFees = order.OtherFees;
            Paid = order.Paid;
            Status = order.Status;
            Notes = order.Notes;
            CreatedAt = order.CreatedAt;
        }
        public int OrderId { get; init; }// 订单编号
        public DateTime? ActualStartingTime { get; set; }// 实际起始时间
        public DateTime? ActualReturnTime { get; set; }// 实际归还时间
        public int TheVehicle { get; set; }// 租用车辆
        public int TheRentalLocation { get; set; }// 租车点（StoreId）
        public string UserName { get; set; }// 用户姓名
        public string UserPhone { get; set; }// 用户手机号
        public bool LongTermLease { get; set; } = false;// 长租
        public decimal Deposit { get; set; }// 押金
        public decimal Rent { get; set; }// 租金
        public decimal DispatchFee { get; set; }// 调度费
        public decimal OtherFees { get; set; }// 其他费用
        public decimal Paid { get; set; }// 已付
        public Order.OrderStatus Status { get; set; }// 订单状态
        public string? Notes { get; set; }// 备注
        public DateTime CreatedAt { get; set; }
    }
}
