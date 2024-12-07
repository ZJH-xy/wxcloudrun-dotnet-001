using aspnetapp.Dao.RepositoryInterface.Web;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;

namespace aspnetapp.Controllers.Web {
    public class StoreControllerWeb : Controller {
        private readonly MyDbContext _context;
        private readonly ILogger<StoreControllerWeb> _logger;
        private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

        public StoreControllerWeb(MyDbContext context, ILogger<StoreControllerWeb> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            _context = context;
            _logger = logger;
            _wxSetting = wxSetting;
        }


        // 获取所有门店列表
        public async Task<List<Store>> GetAllList() {
            return await _context.Store.Where(s => !s.IsDelete).ToListAsync();
        }

        // 获取特定门店通过ID
        public async Task<Store?> GetById(int id) {
            return await _context.Store.FindAsync(id);
        }

        // 获取总门店数，用于分页
        public async Task<int> GetPageSum() {
            return await _context.Store.Where(s => !s.IsDelete).CountAsync();
        }

        // 获取分页后的门店数据
        public async Task<List<Store>> GetTablePage(int limit, int pageIndex) {
            return await _context.Store
                .Where(s => !s.IsDelete)
                .OrderBy(s => s.Id)
                .Skip((pageIndex - 1) * limit)
                .Take(limit)
                .ToListAsync();
        }

        // 获取门店表结构（字段信息）
        public async Task<List<string>> GetTableStructure() {
            var properties = typeof(Store).GetProperties();
            List<string> structure = properties.Select(prop => $"{prop.Name} ({prop.PropertyType.Name})").ToList();
            return await Task.FromResult(structure);
        }

        // 更新门店信息
        public async Task<IActionResult> UpdateStore(Store updatedStore) {
            _logger.LogInformation("正在启动ID为{StoreId}的门店更新过程", updatedStore.Id);

            var store = await _context.Store.FindAsync(updatedStore.Id);
            if (store == null) {
                _logger.LogWarning("Store with ID {StoreId} not found", updatedStore.Id);
                return NotFound("Store not found.");
            }

            // 更新门店属性
            store.Name = updatedStore.Name;
            store.BusinessHoursStart = updatedStore.BusinessHoursStart;
            store.BusinessHoursEnd = updatedStore.BusinessHoursEnd;
            store.BusinessStatus = updatedStore.BusinessStatus;
            store.Telephone = updatedStore.Telephone;
            store.WeChat = updatedStore.WeChat;
            store.Address = updatedStore.Address;
            store.GpsLongitude = updatedStore.GpsLongitude;
            store.GpsLatitude = updatedStore.GpsLatitude;
            store.Pictures = updatedStore.Pictures;
            store.Introduce = updatedStore.Introduce;
            store.UpdatedAt = DateTime.Now;

            // 设置并发标记
            _context.Entry(store).Property("RowVersion").OriginalValue = updatedStore.RowVersion;

            try {
                _context.Store.Update(store);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Store with ID {StoreId} updated successfully", updatedStore.Id);
                return Ok("Store updated successfully.");
            } catch (DbUpdateConcurrencyException) {
                _logger.LogWarning("并发冲突发生在更新门店时 {StoreId}", updatedStore.Id);
                return Conflict("Update failed due to concurrent changes.");
            } catch (DbUpdateException ex) {
                _logger.LogError(ex, "Error updating store with ID {StoreId}", updatedStore.Id);
                return StatusCode(500, "Error updating store.");
            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error while updating store with ID {StoreId}", updatedStore.Id);
                return StatusCode(500, "Unexpected error updating store.");
            }
        }

        // 根据条件进行门店搜索
        public async Task<List<Store>> SearchStores(string? name = null, string? address = null, string sortField = "Id", string sortOrder = "asc") {
            _logger.LogInformation("[SearchStores] Starting search with filters - Name: {Name}, Address: {Address}, SortField: {SortField}, SortOrder: {SortOrder}",
                                    name, address, sortField, sortOrder);

            var query = _context.Store.AsQueryable();

            // 过滤条件
            if (!string.IsNullOrEmpty(name)) {
                query = query.Where(s => s.Name.Contains(name));
            }
            if (!string.IsNullOrEmpty(address)) {
                query = query.Where(s => s.Address.Contains(address));
            }

            // 排序逻辑
            query = sortField.ToLower() switch {
                "name" => sortOrder == "asc" ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
                "createdat" => sortOrder == "asc" ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
                "updatedat" => sortOrder == "asc" ? query.OrderBy(s => s.UpdatedAt) : query.OrderByDescending(s => s.UpdatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(s => s.Id) : query.OrderByDescending(s => s.Id),
            };

            List<Store> results = await query.ToListAsync();
            _logger.LogInformation("Found {Count} stores with given filters and sorting", results.Count);

            return results;
        }

        // 删除门店（逻辑删除）
        public async Task<IActionResult> DeleteStore(int storeId) {
            var store = await _context.Store.FindAsync(storeId);
            if (store == null) {
                _logger.LogWarning("Store with ID {StoreId} not found", storeId);
                return NotFound("Store not found.");
            }

            store.IsDelete = true;
            store.UpdatedAt = DateTime.Now;

            try {
                _context.Store.Update(store);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Store with ID {StoreId} deleted successfully", storeId);
                return Ok("Store deleted successfully.");
            } catch (Exception ex) {
                _logger.LogError(ex, "Error deleting store with ID {StoreId}", storeId);
                return StatusCode(500, "Error deleting store.");
            }
        }

        /// <summary>
        /// 更新门店图片信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="fileId"></param>
        /// <returns>返回更改行数，负数错误</returns>
        public async Task<int> PutStoreImagePath(int storeId, string fileId) {
            var store = await _context.Store.SingleOrDefaultAsync(s => s.Id == storeId);

            if (store == null)
                return -1; // 门店不存在

            // 如果门店已有图片，先删除原图片
            if (!string.IsNullOrEmpty(store.Pictures)) {
                var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
                var appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;
                var envId = _wxSetting.Value.Env;

                List<string> fileid_list = new() { store.Pictures! };

                // 删除原图片
                var re = await TcbApi.BatchDeleteFileAsync(appId, envId, fileid_list);
                if (re.errcode != ReturnCode.请求成功) {
                    _logger.LogError("删除门店{StoreId}原图片{ImageId}", store.Id, store.Pictures);
                    // 可以选择返回错误码或继续执行其他操作
                }
            }

            // 更新门店图片路径
            store.Pictures = fileId;
            store.UpdatedAt = DateTime.Now;
            try {
                _context.Store.Update(store);
                await _context.SaveChangesAsync();
            } catch (Exception e) {
                _logger.LogError(e, "保存门店{StoreId}图片路径", store);
                return -2; // 更新失败
            }

            return 0; // 更新成功
        }
    }
}
