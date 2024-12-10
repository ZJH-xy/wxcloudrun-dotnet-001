using aspnetapp.Controllers.API.Miniprogram;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;

namespace aspnetapp.Controllers.Web {
    /// <summary>
    /// 套餐
    /// </summary>
    public class StoreAccountControllerWeb : Controller {
        private readonly MyDbContext _context;
        private readonly ILogger<StoreAccountControllerWeb> _logger;
        private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

        public StoreAccountControllerWeb(MyDbContext context, ILogger<StoreAccountControllerWeb> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            _context = context;
            _logger = logger;
            _wxSetting = wxSetting;
        }


        // 获取所有套餐列表
        public async Task<List<StoreAccount>> GetAllList() {
            return await _context.StoreAccount.ToListAsync();
        }

        // 获取特定套餐通过ID
        public async Task<StoreAccount?> GetById(int id) {
            return await _context.StoreAccount.FindAsync(id);
        }

        // 获取总套餐数，用于分页
        public async Task<int> GetPageSum() {
            return await _context.StoreAccount.CountAsync();
        }

        // 获取分页后的套餐数据
        public async Task<List<StoreAccount>> GetTablePage(int limit, int pageIndex) {
            return await _context.StoreAccount
                .OrderBy(s => s.Id)
                .Skip((pageIndex - 1) * limit)
                .Take(limit)
                .ToListAsync();
        }

        // 获取套餐表结构（字段信息）
        public async Task<List<string>> GetTableStructure() {
            var properties = typeof(StoreAccount).GetProperties();
            List<string> structure = properties.Select(prop => $"{prop.Name} ({prop.PropertyType.Name})").ToList();
            return await Task.FromResult(structure);
        }

        // 更新套餐信息
        public async Task<IActionResult> UpdateStoreAccount(StoreAccount updatedStoreAccount) {
            var StoreAccount = await _context.StoreAccount.FindAsync(updatedStoreAccount.Id);
            if (StoreAccount == null) {
                _logger.LogWarning("StoreAccount with ID {StoreAccountId} not found", updatedStoreAccount.Id);
                return NotFound("StoreAccount not found.");
            }

            // 更新套餐属性
            StoreAccount.TheStore = updatedStoreAccount.TheStore;
            StoreAccount.Account = updatedStoreAccount.Account;
            StoreAccount.Password = UserAPI.HashPassword(updatedStoreAccount.Password);

            // 设置并发标记
            _context.Entry(StoreAccount).Property("RowVersion").OriginalValue = updatedStoreAccount.RowVersion;

            try {
                _context.StoreAccount.Update(StoreAccount);
                await _context.SaveChangesAsync();
                _logger.LogInformation("StoreAccount with ID {StoreAccountId} updated successfully", updatedStoreAccount.Id);
                return Ok("StoreAccount updated successfully.");
            } catch (DbUpdateConcurrencyException) {
                _logger.LogWarning("并发冲突发生在更新套餐时 {StoreAccountId}", updatedStoreAccount.Id);
                return Conflict("Update failed due to concurrent changes.");
            } catch (DbUpdateException ex) {
                _logger.LogError(ex, "Error updating StoreAccount with ID {StoreAccountId}", updatedStoreAccount.Id);
                return StatusCode(500, "Error updating StoreAccount.");
            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error while updating StoreAccount with ID {StoreAccountId}", updatedStoreAccount.Id);
                return StatusCode(500, "Unexpected error updating StoreAccount.");
            }
        }

        // 添加套餐
        public async Task<IActionResult> AddStoreAccountAsync(StoreAccount newStoreAccount) {
            if (!ModelState.IsValid) {
                return BadRequest("套餐信息无效");
            }

            try {
                // 这里保存套餐到数据库
                _context.StoreAccount.Add(newStoreAccount);
                await _context.SaveChangesAsync();

                return Ok("套餐添加成功");
            } catch (Exception ex) {
                return StatusCode(500, $"添加套餐时发生错误: {ex.Message}");
            }
        }

        // 根据条件进行套餐搜索
        public async Task<List<StoreAccount>> SearchStoreAccounts(int? theStore = null, string? account = null, string sortField = "Id", string sortOrder = "asc") {
            var query = _context.StoreAccount.AsQueryable();

            // 过滤条件
            if (theStore > 0) {
                query = query.Where(s => s.TheStore == theStore);
            }
            if (!string.IsNullOrEmpty(account)) {
                query = query.Where(s => s.Account.Contains(account));
            }

            // 排序逻辑
            query = sortField.ToLower() switch {
                "thestore" => sortOrder == "asc" ? query.OrderBy(s => s.TheStore) : query.OrderByDescending(s => s.TheStore),
                _ => sortOrder == "asc" ? query.OrderBy(s => s.Id) : query.OrderByDescending(s => s.Id),
            };

            List<StoreAccount> results = await query.ToListAsync();
            _logger.LogInformation("Found {Count} StoreAccounts with given filters and sorting", results.Count);

            return results;
        }
    }
}
