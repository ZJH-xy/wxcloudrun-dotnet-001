using aspnetapp.Models;
using aspnetapp.Dao.RepositoryInterface.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace aspnetapp.Controllers.Web {
	public class OrderControllerWeb : Controller {

		private readonly MyDbContext _context;
		private readonly ILogger<OrderControllerWeb> _logger;

		public OrderControllerWeb(MyDbContext context, ILogger<OrderControllerWeb> logger) {
			_context = context;
			_logger = logger;
		}

		/// <summary>
		/// 获取所有订单
		/// </summary>
		public async Task<List<Order>> GetAllOrders() {
			return await _context.Order.ToListAsync();
		}

		// 根据月份获取订单列表
		public async Task<List<Order>> GetAllOrdersByMonth(DateTime startOfMonth, DateTime endOfMonth) {
			// 查询该月份内的所有订单
			var ordersInMonth = await _context.Order
				.Where(o => o.CreatedAt >= startOfMonth && o.CreatedAt <= endOfMonth)
				.ToListAsync();

			return ordersInMonth;
		}

		/// <summary>
		/// 根据订单ID获取订单
		/// </summary>
		public async Task<Order?> GetOrderById(int id) {
			return await _context.Order.FindAsync(id);
		}

		/// <summary>
		/// 获取订单总数
		/// </summary>
		public async Task<int> GetOrderCount() {
			return await _context.Order.CountAsync();
		}

		/// <summary>
		/// 分页查询订单
		/// </summary>
		public async Task<List<Order>> GetTablePage(int limit, int pageIndex) {
			var orders = await _context.Order
				.OrderBy(o => o.Id) // 根据订单ID排序，确保分页顺序一致
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();

			// 更新门店名称
			await FillOrderNamesAsync(orders);

			return orders;
		}

		/// <summary>
		/// 查询订单（支持多字段搜索）
		/// </summary>
		public async Task<(List<Order>, int sum)> SearchOrders(int limit, int pageIndex, string? userPhone = null, string? status = null, string? theVehicle = null, string? theRentalLocation = null, string? searchStoreName = null, string sortField = "Id", string sortOrder = "asc") {
			if (userPhone.IsNullOrEmpty() && status.IsNullOrEmpty() && theVehicle.IsNullOrEmpty() && theRentalLocation.IsNullOrEmpty() && searchStoreName.IsNullOrEmpty() && sortField == "Id" && sortOrder == "asc") {
				return (await GetTablePage(limit, pageIndex), limit);
			}

			var query = _context.Order.AsQueryable();

			// 取消跟踪实体
			query.AsNoTracking();

			// 过滤条件
			if (!string.IsNullOrEmpty(userPhone)) {
				query = query.Where(o => o.UserPhone.Contains(userPhone));
			}
			if (!string.IsNullOrEmpty(status)) {
				if (Enum.TryParse<Order.EOrderStatus>(status, out var parsedStatus)) {
					query = query.Where(o => o.Status == parsedStatus);
				}
			}
			if (!string.IsNullOrEmpty(theVehicle) && int.TryParse(theVehicle, out int vehicleId)) {
				query = query.Where(o => o.TheVehicle == vehicleId);// 车辆
			}
			if (!string.IsNullOrEmpty(theRentalLocation) && int.TryParse(theRentalLocation, out int RentalLocationId)) {
				query = query.Where(o => o.TheRentalLocation == RentalLocationId);// 租车点
			}

			// 门店名称过滤
			if (!string.IsNullOrEmpty(searchStoreName)) {
				var storeIds = await _context.Store
					.Where(s => s.Name.Contains(searchStoreName))
					.Select(s => s.Id)
					.ToListAsync();

				query = query.Where(o => storeIds.Contains(o.TheRentalLocation) ||
										 (o.TheReturnThePoint.HasValue && storeIds.Contains(o.TheReturnThePoint.Value)));
			}

			// 排序逻辑
			query = sortField.ToLower() switch {
				"createdat" => sortOrder == "asc" ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt),
				"updatedat" => sortOrder == "asc" ? query.OrderBy(o => o.UpdatedAt) : query.OrderByDescending(o => o.UpdatedAt),
				"actualStartingTime" => sortOrder == "asc" ? query.OrderBy(o => o.ActualStartingTime) : query.OrderByDescending(o => o.ActualStartingTime),
				"actualReturnTime" => sortOrder == "asc" ? query.OrderBy(o => o.ActualReturnTime) : query.OrderByDescending(o => o.ActualReturnTime),
				"successTime" => sortOrder == "asc" ? query.OrderBy(o => o.SuccessTime) : query.OrderByDescending(o => o.SuccessTime),
				_ => sortOrder == "asc" ? query.OrderBy(o => o.Id) : query.OrderByDescending(o => o.Id),
			};

			int sum = await query.CountAsync();

			List<Order> results = await query
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();

			// 更新门店名称
			await FillOrderNamesAsync(results);

			return (results, sum);
		}

		/// <summary>
		/// 更新订单信息
		/// </summary>
		public async Task<IActionResult> UpdateOrder(Order updatedOrder) {
			_logger.LogInformation("正在更新订单，订单ID：{OrderId}", updatedOrder.Id);

			var order = await _context.Order.FindAsync(updatedOrder.Id);
			if (order == null) {
				_logger.LogWarning("未找到订单，订单ID：{OrderId}", updatedOrder.Id);
				return NotFound("未找到指定的订单。");
			}

			// 更新订单属性

			// 检查费用更改
			if (order.Deposit != updatedOrder.Deposit || order.Rent != updatedOrder.Rent || order.OtherFees != updatedOrder.OtherFees) {
				// 确认状态是否合法
				if (order.Status != Order.EOrderStatus.待付款 && order.Status != Order.EOrderStatus.进行中 && order.Status != Order.EOrderStatus.侍补余) {
					_logger.LogWarning("尝试在非法状态下更新订单费用，订单ID：{OrderId}, 当前状态：{OrderStatus}", updatedOrder.Id, order.Status);
					return StatusCode(403, "订单完成前才可修改费用，请确保订单状态为待付款、进行中或侍补余。");
				}

				order.Deposit = updatedOrder.Deposit;
				order.Rent = updatedOrder.Rent;
				order.OtherFees = updatedOrder.OtherFees;
			}

			order.Notes = updatedOrder.Notes;
			order.UpdatedAt = DateTime.Now;

			// 设置并发标记
			_context.Entry(order).Property("RowVersion").OriginalValue = updatedOrder.RowVersion;

			try {
				_context.Order.Update(order);
				await _context.SaveChangesAsync();
				_logger.LogInformation("订单更新成功，订单ID：{OrderId}", updatedOrder.Id);
				return Ok("订单更新成功。");
			} catch (DbUpdateConcurrencyException) {
				_logger.LogWarning("更新订单时发生并发冲突，订单ID：{OrderId}", updatedOrder.Id);
				return Conflict("更新失败，记录已被其他用户修改。");
			} catch (DbUpdateException ex) {
				_logger.LogError(ex, "更新订单时发生数据库错误，订单ID：{OrderId}", updatedOrder.Id);
				return StatusCode(500, "更新订单时发生数据库错误。");
			} catch (Exception ex) {
				_logger.LogError(ex, "更新订单时发生未知错误，订单ID：{OrderId}", updatedOrder.Id);
				return StatusCode(500, "更新订单时发生未知错误。");
			}
		}

		/// <summary>
		/// 更新订单图片信息（如订单相关的图片）
		/// </summary>
		public async Task<int> UpdateOrderImages(int orderId, string fileId) {
			var order = await _context.Order.SingleOrDefaultAsync(o => o.Id == orderId);

			if (order == null)
				return -1;

			// 假设订单有图片字段，这里更新图片
			order.Notes = fileId; // 作为示例，假设文件ID存在于 Notes 字段

			try {
				_context.Order.Update(order);
				await _context.SaveChangesAsync();
				return 0; // 返回成功
			} catch (Exception e) {
				_logger.LogError(e, "Error updating images for order {OrderId}", orderId);
				return -2; // 返回失败
			}
		}

		/// <summary>
		/// 填充订单的关联名称（门店名称和车辆车牌号）
		/// </summary>
		/// <param name="orders">订单列表</param>
		/// <returns>填充好名称的订单列表</returns>
		public async Task<List<Order>> FillOrderNamesAsync(List<Order> orders) {
			if (orders == null || !orders.Any())
				return orders;

			// 获取车辆ID列表
			var vehicleIds = orders
				.Select(o => o.TheVehicle)
				.Distinct()
				.ToList();

			// 获取门店ID列表
			var storeIds = orders
				.SelectMany(o => new[] { o.TheRentalLocation, o.TheReturnThePoint })
				.Distinct()
				.ToList();

			// 查询门店名称
			var stores = await _context.Store
				.Where(s => storeIds.Contains(s.Id))
				.ToDictionaryAsync(s => s.Id, s => s.Name);

			// 查询车辆车牌号
			var vehicles = await _context.Vehicle
				.Where(v => vehicleIds.Contains(v.Id))
				.ToDictionaryAsync(v => v.Id, v => v.PlateNumber);

			// 替换订单中的门店ID和车辆ID为名称和车牌号
			foreach (var order in orders) {
				if (stores.TryGetValue(order.TheRentalLocation, out var rentalLocationName)) {
					order.RentalLocationName = rentalLocationName; // 设置租车点名称
				}

				//if (stores.TryGetValue(order.TheReturnThePoint, out var returnPointName)) {
				//	order.ReturnThePointName = returnPointName; // 设置还车点名称
				//}

				if (order.TheReturnThePoint.HasValue && stores.TryGetValue(order.TheReturnThePoint.Value, out var returnPointName)) {
					order.ReturnThePointName = returnPointName; // 设置还车点名称
				}

				if (vehicles.TryGetValue(order.TheVehicle, out var plateNumber)) {
					order.VehiclePlateNumber = plateNumber; // 设置车辆车牌号
				}
			}

			return orders;
		}

	}
}
