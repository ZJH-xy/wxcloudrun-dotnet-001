using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using aspnetapp.Models;
using static aspnetapp.Models.Order;
using Microsoft.CodeAnalysis;

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

        /// <summary>
        /// 商家登录
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [AllowAnonymous]// 允许匿名访问
        [HttpGet("login/{account}/{password}")]
        public async Task<IActionResult> Login(string account, string password) {
            using MyDbContext dbcontext = new();
            if (password is null)
                return StatusCode(403, "密码为空");

            /* 商家帐号判断 */
            Models.StoreAccount? storeAccount;
            try {
                storeAccount = await dbcontext.StoreAccount.SingleOrDefaultAsync(sa => sa.Account == account && sa.Password == password);

            } catch (Exception e) {
                _logger.LogError(e, "查询商家帐号");
                return StatusCode(500);
            }

            if (storeAccount is null)
                return StatusCode(403, "帐号或密码错误");

            /* 门店判断 */
            if (await dbcontext.Store.AnyAsync(s => s.Id == storeAccount.Id && s.IsDelete))// 门店是否删除
                return StatusCode(403, "门店不存在");

            return StatusCode(200, GetJwtToken(CreateClaim(storeAccount.Id.ToString(), "store")));
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

            if (order.Status != Order.OrderStatus.待确认)
                return StatusCode(403, "订单状态异常");

            return StatusCode(200, new ReturnOrder(order));
        }

        /// <summary>
        /// 商家确认订单
        /// </summary>
        /// <param name="getData"></param>
        /// <returns></returns>
        [HttpPost("confirm/order")]
        public async Task<IActionResult> ConfirmAnOrder(GetConfirmOrder getData) {
            using MyDbContext dbcontext = new();

            /* 检查订单状态 */
            Order? order;
            try {
                order = await dbcontext.Order.FindAsync(getData.OderId);

            } catch (Exception e) {
                _logger.LogError(e, "获取订单{OderId}", getData.OderId);
                return StatusCode(500);
            }

            if (order is null)
                return StatusCode(403, "订单不存在");

            if (order.Status != Order.OrderStatus.待确认)
                return StatusCode(403, "订单状态异常");

            /* 检查车辆状态 */
            Vehicle? vehicle;
            try {
                vehicle = await dbcontext.Vehicle.FindAsync(getData.TheVehicle);

            } catch (Exception e) {
                _logger.LogError(e, "查询车辆{VehicleId}", getData.TheVehicle);
                return StatusCode(500);
            }

            if (vehicle is null)
                return StatusCode(403, "车辆不存在");

            // 检查车辆是否属于当前商家
            int storeId;
            try {
                storeId = await dbcontext.StoreAccount.Where(sa => sa.Id == GetUserIdInt()).Select(sa => sa.TheStore).FirstAsync();

            } catch (Exception e) {
                _logger.LogError(e, "获取商家{VehicleId}的门店Id", GetUserIdInt());
                return StatusCode(500);
            }
            if (vehicle.TheCurrentStore != storeId)// 车辆当前所在门店
                return StatusCode(403, "车辆不在当前门店");

            if (vehicle.State != Vehicle.Estates.锁定)
                return StatusCode(403, "车辆状态异常");

            using var transaction = await dbcontext.Database.BeginTransactionAsync();// 事务开始

            /* 确认订单 */
            order.Status = Order.OrderStatus.进行中;
            vehicle.State = Vehicle.Estates.已出租;
            try {
                await dbcontext.SaveChangesAsync();
                await transaction.CommitAsync();// 提交事务

            } catch (Exception e) {
                _logger.LogError(e, "确认订单事务失败，订单{OrderId}，车辆{VehicleId}状态更改", order.Id, vehicle.Id);
                await transaction.RollbackAsync();// 回滚事务
                return StatusCode(500);
            }

            return StatusCode(200);
        }

        /// <summary>
        /// 商家获取换车信息
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [HttpGet("confirm/vehicle/replacement/{orderId}")]
        public async Task<IActionResult> GetConfirmVehicleReplacement(int orderId) {
            using MyDbContext dbcontext = new();
            VehicleReplacementRecord? vrr;
            try {
                vrr = await dbcontext.VehicleReplacementRecord.FirstOrDefaultAsync(v => v.TheOrder == orderId);

            } catch (Exception e) {
                _logger.LogError(e, "查询换车请求{OrderId}", orderId);
                return StatusCode(500);
            }

            return StatusCode(200, vrr);
        }

        /// <summary>
        /// 商家确认换车
        /// </summary>
        /// <returns></returns>
        [HttpPost("confirm/vehicle/replacement")]
        public async Task<IActionResult> ConfirmVehicleReplacement(GetConfirmReplacement getData) {
            using MyDbContext dbcontext = new();

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

            if (order.Status != Order.OrderStatus.进行中)
                return StatusCode(403, "订单状态异常");

            /* 检查换车记录 */
            VehicleReplacementRecord? vrr;
            try {
                // 查询当前订单的换车请求
                vrr = await dbcontext.VehicleReplacementRecord.FirstAsync(v => v.TheOrder == getData.OderId);

            } catch (Exception e) {
                _logger.LogError(e, "换车记录表OrderId=={OrderId}查询", getData.OderId);
                return StatusCode(500);
            }

            if (vrr is null)
                return StatusCode(403, "换车请求不存在");

            if (vrr.State != VehicleReplacementRecord.Estates.侍确认)
                return StatusCode(403, "请求状态异常");

            /* 检查车辆状态 */
            Vehicle? newVehicle;
            try {
                // 查询将要更换的车辆
                newVehicle = await dbcontext.Vehicle.FindAsync(getData.TheVehicle);

            } catch (Exception e) {
                _logger.LogError(e, "查询车辆{VehicleId}", getData.TheVehicle);
                return StatusCode(500);
            }

            if (newVehicle is null)
                return StatusCode(403, "车辆不存在");

            // 检查车辆是否属于当前商家
            int storeId;
            try {
                storeId = await dbcontext.StoreAccount.Where(sa => sa.Id == GetUserIdInt()).Select(sa => sa.TheStore).FirstAsync();

            } catch (Exception e) {
                _logger.LogError(e, "根据商家帐号Id{VehicleReplacementRecordId}获取门店", GetUserIdInt());
                return StatusCode(500);
            }
            if (newVehicle.TheCurrentStore != storeId)// 车辆当前所在门店
                return StatusCode(403, "车辆不在当前门店");

            if (newVehicle.State != Vehicle.Estates.空闲)
                return StatusCode(403, "车辆状态异常");

            // 获取用户将要更换的旧车辆
            Vehicle? oldVehicle;
            try {
                oldVehicle = await dbcontext.Vehicle.FirstAsync(v => v.Id == order.TheVehicle);

            } catch (Exception e) {
                _logger.LogError(e, "获取车辆{VehicleId}", GetUserIdInt());
                return StatusCode(500);
            }

            using var transaction = await dbcontext.Database.BeginTransactionAsync();// 事务开始

            DateTime now = DateTime.Now;
            /* 更改租用车辆 */
            order.TheVehicle = newVehicle.Id;// 更改订单租用车辆
            order.UpdatedAt = now;

            /* 车辆状态改为侍确认 */
            oldVehicle.State = Vehicle.Estates.侍确认;
            oldVehicle.UpdatedAt = now;

            /* 换车记录表VehicleReplacementRecord 更新 */
            vrr.State = VehicleReplacementRecord.Estates.已完成;
            vrr.UpdatedAt = now;

            try {
                await dbcontext.SaveChangesAsync();
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
            using MyDbContext dbcontext = new();



            return StatusCode(200);
        }

        /// <summary>
        /// 商家确认还车
        /// </summary>
        /// <returns></returns>
        [HttpPost("confirm/vehicle/return")]
        public async Task<IActionResult> ConfirmVehicleReturn() {
            using MyDbContext dbcontext = new();

            /* 检查订单状态 */

            /* 判断超时 */

            /* 判断调度费 */

            /* 计算总价，更新订单价格 */

            /* 退还押金或支付差价 */


            /* 车辆状态改为侍确认 */

            /* 订单表Order 状态更新 */

            /* 营业额统计表RevenueStatistics 更新 */

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
    /// 商家确认订单返回格式
    /// </summary>
    public struct ReturnOrder {
        public ReturnOrder(Order order) {
            Id = order.Id;
            TheVehicle = order.TheVehicle;
            TheRentalLocation = order.TheRentalLocation;
            UserName = order.UserName;
            UserPhone = order.UserPhone;
            Deposit = order.Deposit;
            Rent = order.Rent;
            Paid = order.Paid;
            Status = order.Status;
            CreatedAt = order.CreatedAt;
            Notes = order.Notes;
        }
        public int Id { get; init; }// 订单编号
        public int TheVehicle { get; set; }// 租用车辆
        public int TheRentalLocation { get; set; }// 租车点（StoreId）
        public string UserName { get; set; }// 用户姓名
        public string UserPhone { get; set; }// 用户手机号
        public decimal Deposit { get; set; }// 押金
        public decimal Rent { get; set; }// 租金
        public decimal Paid { get; set; }// 已付
        public Order.OrderStatus Status { get; set; }// 订单状态
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }// 备注
        //public DateTime UpdatedAt { get; set; }
        //public string? IdentityCard { get; set; }// 身份证号
    }
}
