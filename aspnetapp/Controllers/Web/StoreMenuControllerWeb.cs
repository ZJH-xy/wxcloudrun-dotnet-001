using aspnetapp.Controllers.API.Miniprogram;
using aspnetapp.Dao.RepositoryInterface.Web;
using aspnetapp.Models;
using aspnetapp.Pages;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;

namespace aspnetapp.Controllers.Web {
    public class StoreMenuControllerWeb : Controller {

        private readonly MyDbContext _context;
        private readonly ILogger<StoreMenuControllerWeb> _logger;
        private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

        public StoreMenuControllerWeb(MyDbContext context, ILogger<StoreMenuControllerWeb> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            _context = context;
            _logger = logger;
            _wxSetting = wxSetting;
        }

        public async Task<List<StoreMenu>> GetAllList() {
            return await _context.StoreMenus.Where(s => !s.IsDelete).ToListAsync();
        }

        public async Task<StoreMenu?> GetById(int id) {
            return await _context.StoreMenus.FindAsync(id);
        }

        public async Task<int> GetPageSum() {
            return await _context.StoreMenus.Where(s => !s.IsDelete).CountAsync();
        }

        public async Task<List<StoreMenu>> GetTablePage(int limit, int pageIndex) {
			// 查询分页数据
			var storeMenus = await _context.StoreMenus
				.Where(s => !s.IsDelete)
				.OrderBy(u => u.Id) // 根据主键排序，确保分页顺序一致
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();

			// 更新门店名称
			await UpdateStoreNames(storeMenus);

			return storeMenus;
		}

        // 添加门店
        public async Task<IActionResult> AddStoreAsync(StoreMenu newStore) {
            if (!ModelState.IsValid) {
                return BadRequest("门店信息无效");
            }

            try {
                // 这里保存门店到数据库
                _context.StoreMenus.Add(newStore);
                await _context.SaveChangesAsync();

                return Ok("门店添加成功");
            } catch (Exception ex) {
                return StatusCode(500, $"添加门店时发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除门店（逻辑删除）
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        public async Task<IActionResult> DeleteStore(int storeId) {
            var store = await _context.StoreMenus.FindAsync(storeId);
            if (store == null) {
                _logger.LogWarning("StoreMenu with ID {StoreId} not found", storeId);
                return NotFound("StoreMenu not found.");
            }

            store.IsDelete = true;

            try {
                _context.StoreMenus.Update(store);
                await _context.SaveChangesAsync();
                _logger.LogInformation("StoreMenu with ID {StoreId} deleted successfully", storeId);
                return Ok("StoreMenu deleted successfully.");
            } catch (Exception ex) {
                _logger.LogError(ex, "Error deleting StoreMenu with ID {StoreId}", storeId);
                return StatusCode(500, "Error deleting StoreMenu.");
            }
        }

        /// <summary>
        /// 更新套餐信息
        /// </summary>
        /// <param name="updatedStoreMenu"></param>
        /// <returns></returns>
        public async Task<IActionResult> UpdateStoreMenu(StoreMenu updatedStoreMenu) {
            _logger.LogInformation("正在启动ID为{StoreMenuId}的套餐的更新过程", updatedStoreMenu.Id);

            var user = await _context.StoreMenus.FindAsync(updatedStoreMenu.Id);
            if (user == null) {
                _logger.LogWarning("StoreMenu with ID {StoreMenuId} not found", updatedStoreMenu.Id);
                return NotFound("StoreMenu not found.");
            }

            // 更新套餐属性
            user.TheStore = updatedStoreMenu.TheStore;
            user.Duration = updatedStoreMenu.Duration;
            user.Rent = updatedStoreMenu.Rent;
            user.Deposit = updatedStoreMenu.Deposit;

            // 设置并发标记
            _context.Entry(user).Property("RowVersion").OriginalValue = updatedStoreMenu.RowVersion;

            try {
                _context.StoreMenus.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("StoreMenu with ID {StoreMenuId} updated successfully", updatedStoreMenu.Id);
                return Ok("StoreMenu updated successfully.");

            } catch (DbUpdateConcurrencyException) {
                _logger.LogWarning("使用ID更新套餐时发生并发冲突 {StoreMenuId}", updatedStoreMenu.Id);
                return Conflict("Update failed due to concurrent changes.");

            } catch (DbUpdateException ex) {
                _logger.LogError(ex, "Error updating StoreMenu with ID {StoreMenuId}", updatedStoreMenu.Id);
                return StatusCode(500, "Error updating StoreMenu.");

            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error while updating StoreMenu with ID {StoreMenuId}", updatedStoreMenu.Id);
                return StatusCode(500, "Unexpected error updating StoreMenu.");
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="theStore"></param>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        public async Task<(List<StoreMenu>, int sum)> SearchStoreMenus(int limit, int pageIndex, string? theStore = null, string sortField = "Id", string sortOrder = "asc") {
			if ((theStore.IsNullOrEmpty() && sortField == "Id" && sortOrder == "asc")) {
				var list = await GetTablePage(limit, pageIndex);
				return (list, list is null ? 0 : list.Count);
			}

			var query = _context.StoreMenus.AsQueryable();

            query.AsNoTracking();

            // 过滤条件
            if (!string.IsNullOrEmpty(theStore) && int.TryParse(theStore, out int storeId)) {
                query = query.Where(u => u.TheStore == storeId);
            }

            // 排序逻辑
            query = sortField.ToLower() switch {
                "thestore" => sortOrder == "asc" ? query.OrderBy(u => u.TheStore) : query.OrderByDescending(u => u.TheStore),
                _ => sortOrder == "asc" ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id),
            };

            // 过滤掉已删除的记录
            query = query.Where(s => !s.IsDelete);

			int sum = await query.CountAsync();

			List<StoreMenu> results = await query
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();


			// 查询门店名称并更新结果
			await UpdateStoreNames(results);

			return (results, sum);
        }

		private async Task UpdateStoreNames(List<StoreMenu> storeMenus) {
			// 获取所有涉及的门店 ID
			var storeIds = storeMenus
				.Select(sm => sm.TheStore)
				.Distinct()
				.ToList();

			// 查询门店名称
			var stores = await _context.Store
				.Where(s => storeIds.Contains(s.Id))
				.ToDictionaryAsync(s => s.Id, s => s.Name);

			// 替换门店套餐中的门店ID为名称
			foreach (var menu in storeMenus) {
				if (stores.TryGetValue(menu.TheStore, out var storeName)) {
					menu.StoreName = storeName; // 假设 StoreMenu 添加了 StoreName 属性
				}
			}
		}
	}
}
