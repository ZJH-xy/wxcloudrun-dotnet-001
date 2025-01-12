using aspnetapp.Controllers.API.Miniprogram;
using aspnetapp.Pages;
using Microsoft.AspNetCore.Http;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;

namespace aspnetapp.Controllers.Web {
    /// <summary>
    /// 商家账号
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


        // 获取所有帐号列表
        public async Task<List<StoreAccount>> GetAllList() {
            return await _context.StoreAccount.ToListAsync();
        }

        // 获取特定帐号通过ID
        public async Task<StoreAccount?> GetById(int id) {
            return await _context.StoreAccount.FindAsync(id);
        }

        // 获取总帐号数，用于分页
        public async Task<int> GetPageSum() {
            return await _context.StoreAccount.CountAsync();
        }

		/// <summary>
		/// 删除帐号
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<IActionResult> Delete(int id) {
			var sa = await _context.StoreAccount.FindAsync(id);
			if (sa == null) {
				_logger.LogWarning("StoreMenu with ID {StoreId} not found", id);
				return NotFound("StoreMenu not found.");
			}

			try {
				_context.StoreAccount.Remove(sa);
				await _context.SaveChangesAsync();
				_logger.LogInformation("StoreMenu with ID {StoreId} deleted successfully", id);
				return Ok("StoreMenu deleted successfully.");
			} catch (Exception ex) {
				_logger.LogError(ex, "Error deleting StoreMenu with ID {StoreId}", id);
				return StatusCode(500, "Error deleting StoreMenu.");
			}
		}

		// 获取分页后的帐号数据
		public async Task<List<StoreAccount>> GetTablePage(int limit, int pageIndex) {
			// 获取商家帐号列表
			var storeAccounts = await _context.StoreAccount
				.OrderBy(s => s.Id) // 根据主键排序，确保分页顺序一致
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();

			// 查询门店名称并更新结果
			await UpdateStoreNames(storeAccounts);

			return storeAccounts;
		}

        // 获取帐号表结构（字段信息）
        public async Task<List<string>> GetTableStructure() {
            var properties = typeof(StoreAccount).GetProperties();
            List<string> structure = properties.Select(prop => $"{prop.Name} ({prop.PropertyType.Name})").ToList();
            return await Task.FromResult(structure);
        }

        // 更新帐号信息
        public async Task<IActionResult> UpdateStoreAccount(StoreAccount updatedStoreAccount) {
            var StoreAccount = await _context.StoreAccount.FindAsync(updatedStoreAccount.Id);
            if (StoreAccount == null) {
                _logger.LogWarning("StoreAccount with ID {StoreAccountId} not found", updatedStoreAccount.Id);
                return NotFound("StoreAccount not found.");
            }

            // 更新帐号属性
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
                _logger.LogWarning("并发冲突发生在更新帐号时 {StoreAccountId}", updatedStoreAccount.Id);
                return Conflict("Update failed due to concurrent changes.");
            } catch (DbUpdateException ex) {
                _logger.LogError(ex, "Error updating StoreAccount with ID {StoreAccountId}", updatedStoreAccount.Id);
                return StatusCode(500, "Error updating StoreAccount.");
            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error while updating StoreAccount with ID {StoreAccountId}", updatedStoreAccount.Id);
                return StatusCode(500, "Unexpected error updating StoreAccount.");
            }
        }

        // 添加帐号
        public async Task<IActionResult> AddStoreAccountAsync(StoreAccount newStoreAccount) {
            if (!ModelState.IsValid) {
                return BadRequest("帐号信息无效");
            }

            try {
                // 这里保存帐号到数据库
                _context.StoreAccount.Add(newStoreAccount);
                await _context.SaveChangesAsync();

                return Ok("帐号添加成功");
            } catch (Exception ex) {
                return StatusCode(500, $"添加帐号时发生错误: {ex.Message}");
            }
        }

        // 根据条件进行帐号搜索
        public async Task<(List<StoreAccount>, int sum)> SearchStoreAccounts(int limit, int pageIndex, int? theStore = null, string? account = null, string sortField = "Id", string sortOrder = "asc") {
			if ((theStore is null || theStore == 0) && account.IsNullOrEmpty() && sortField == "Id" && sortOrder == "asc") {
				var list = await GetTablePage(limit, pageIndex);
				return (list, list is null ? 0 : list.Count);
			}

			var query = _context.StoreAccount.AsQueryable();

            query.AsNoTracking();

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

			int sum = await query.CountAsync();

			List<StoreAccount> results = await query
				.Skip((pageIndex - 1) * limit) // 跳过前面页的数据
				.Take(limit) // 获取当前页的数据
				.ToListAsync();

			// 查询门店名称并更新结果
			await UpdateStoreNames(results);

			return (results, sum);
        }

		private async Task UpdateStoreNames(List<StoreAccount> storeAccounts) {
			// 获取所有涉及的门店 ID
			var storeIds = storeAccounts
				.Select(sa => sa.TheStore)
				.Distinct()
				.ToList();

			// 查询门店名称
			var stores = await _context.Store
				.Where(s => storeIds.Contains(s.Id))
				.ToDictionaryAsync(s => s.Id, s => s.Name);

			// 替换商家帐号中的门店ID为名称
			foreach (var account in storeAccounts) {
				if (stores.TryGetValue(account.TheStore, out var storeName)) {
					account.StoreName = storeName; // 设置商家所属门店名称
				}
			}
		}
	}
}
