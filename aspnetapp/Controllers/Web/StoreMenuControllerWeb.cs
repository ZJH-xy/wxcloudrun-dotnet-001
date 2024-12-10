using aspnetapp.Controllers.API.Miniprogram;
using aspnetapp.Dao.RepositoryInterface.Web;
using aspnetapp.Models;
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
            return await _context.StoreMenus
                .Where(s => !s.IsDelete)
                .OrderBy(u => u.Id) // 根据主键排序，确保分页顺序一致
                .Skip((pageIndex - 1) * limit) // 跳过前面页的数据
                .Take(limit) // 获取当前页的数据
                .ToListAsync();
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

        // 删除门店（逻辑删除）
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
        public async Task<List<StoreMenu>> SearchStoreMenus(int? theStore = null, string sortField = "Id", string sortOrder = "asc") {
            var query = _context.StoreMenus.AsQueryable();

            // 过滤条件
            if (theStore > 0) {
                query = query.Where(u => u.TheStore == theStore);
            }

            // 排序逻辑
            query = sortField.ToLower() switch {
                "thestore" => sortOrder == "asc" ? query.OrderBy(u => u.TheStore) : query.OrderByDescending(u => u.TheStore),
                _ => sortOrder == "asc" ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id),
            };

            // 过滤掉已删除的记录
            query = query.Where(s => !s.IsDelete);

            List<StoreMenu> results = await query.ToListAsync();
            _logger.LogInformation("Found {Count} StoreMenus with given filters and sorting", results.Count);

            return results;
        }
    }
}
