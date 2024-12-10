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
		public async Task<List<Order>> GetOrderPage(int limit, int pageIndex) {
			return await _context.Order
				.OrderBy(o => o.Id) // 根据订单ID排序，确保分页顺序一致
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();
		}

		/// <summary>
		/// 查询订单（支持多字段搜索）
		/// </summary>
		public async Task<List<Order>> SearchOrders(string? userPhone = null, string? status = null, string sortField = "Id", string sortOrder = "asc") {
			_logger.LogInformation("[SearchOrders] Starting search with filters - UserPhone: {UserPhone}, Status: {Status}, SortField: {SortField}, SortOrder: {SortOrder}",
								   userPhone, status, sortField, sortOrder);

			var query = _context.Order.AsQueryable();

			// 过滤条件
			if (!string.IsNullOrEmpty(userPhone)) {
				query = query.Where(o => o.UserPhone.Contains(userPhone));
			}
			if (!string.IsNullOrEmpty(status)) {
				if (Enum.TryParse<Order.EOrderStatus>(status, out var parsedStatus)) {
					query = query.Where(o => o.Status == parsedStatus);
				}
			}

			// 排序逻辑
			query = sortField.ToLower() switch {
				"userid" => sortOrder == "asc" ? query.OrderBy(o => o.TheUser) : query.OrderByDescending(o => o.TheUser),
				"transactionid" => sortOrder == "asc" ? query.OrderBy(o => o.TransactionId) : query.OrderByDescending(o => o.TransactionId),
				"createdat" => sortOrder == "asc" ? query.OrderBy(o => o.CreatedAt) : query.OrderByDescending(o => o.CreatedAt),
				"updatedat" => sortOrder == "asc" ? query.OrderBy(o => o.UpdatedAt) : query.OrderByDescending(o => o.UpdatedAt),
				_ => sortOrder == "asc" ? query.OrderBy(o => o.Id) : query.OrderByDescending(o => o.Id),
			};

			List<Order> results = await query.ToListAsync();
			_logger.LogInformation("Found {Count} orders with given filters and sorting", results.Count);

			return results;
		}

		/// <summary>
		/// 更新订单信息
		/// </summary>
		public async Task<IActionResult> UpdateOrder(Order updatedOrder) {
			_logger.LogInformation("Updating order with ID {OrderId}", updatedOrder.Id);

			var order = await _context.Order.FindAsync(updatedOrder.Id);
			if (order == null) {
				_logger.LogWarning("Order with ID {OrderId} not found", updatedOrder.Id);
				return NotFound("Order not found.");
			}

            // 更新订单属性
            order.OtherFees = updatedOrder.OtherFees;
            order.Notes = updatedOrder.Notes;
            order.UpdatedAt = DateTime.Now;

            // 设置并发标记
            _context.Entry(order).Property("RowVersion").OriginalValue = updatedOrder.RowVersion;

			try {
				_context.Order.Update(order);
				await _context.SaveChangesAsync();
				_logger.LogInformation("Order with ID {OrderId} updated successfully", updatedOrder.Id);
				return Ok("Order updated successfully.");
			} catch (DbUpdateConcurrencyException) {
				_logger.LogWarning("Concurrency conflict occurred when updating order with ID {OrderId}", updatedOrder.Id);
				return Conflict("Update failed due to concurrent changes.");
			} catch (DbUpdateException ex) {
				_logger.LogError(ex, "Error updating order with ID {OrderId}", updatedOrder.Id);
				return StatusCode(500, "Error updating order.");
			} catch (Exception ex) {
				_logger.LogError(ex, "Unexpected error updating order with ID {OrderId}", updatedOrder.Id);
				return StatusCode(500, "Unexpected error updating order.");
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
	}
}
