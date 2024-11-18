using aspnetapp.Dao.RepositoryInterface.Web;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Tcb;

namespace aspnetapp.Controllers.Web {
    public class UserControllerWeb : Controller, IUserRepositoryWeb {

        private readonly MyDbContext _context;
        private readonly ILogger<UserControllerWeb> _logger;
        private readonly IOptionsSnapshot<WeixinSetting> _wxSetting;

        public UserControllerWeb(MyDbContext context, ILogger<UserControllerWeb> logger, IOptionsSnapshot<WeixinSetting> wxSetting) {
            _context = context;
            _logger = logger;
            _wxSetting = wxSetting;
        }

        public async Task<User?> GetById(int id) {
            return await _context.User.FindAsync(id);
        }

        public async Task<List<User>> GetTablePage(int limit, int pageIndex) {
            return await _context.User
                .OrderBy(u => u.Id) // 根据主键排序，确保分页顺序一致
                .Skip((pageIndex - 1) * limit) // 跳过前面页的数据
                .Take(limit) // 获取当前页的数据
                .ToListAsync();
        }

        public async Task<List<string>> GetTableStructure() {
            var properties = typeof(User).GetProperties();
            List<string> structure = properties.Select(prop => $"{prop.Name} ({prop.PropertyType.Name})").ToList();
            return await Task.FromResult(structure);
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="updatedUser"></param>
        /// <returns></returns>
        public async Task<IActionResult> UpdateUser(User updatedUser) {
            _logger.LogInformation("Starting update process for user with ID {UserId}", updatedUser.Id);

            var user = await _context.User.FindAsync(updatedUser.Id);
            if (user == null) {
                _logger.LogWarning("User with ID {UserId} not found", updatedUser.Id);
                return NotFound("User not found.");
            }

            // 更新用户属性
            user.Phone = updatedUser.Phone;
            //user.Password = updatedUser.Password;
            user.Name = updatedUser.Name;
            user.IdentityCard = updatedUser.IdentityCard;
            user.IdentityCardPictures = updatedUser.IdentityCardPictures;
            user.Nickname = updatedUser.Nickname;
            user.UpdatedAt = DateTime.Now;

            // 设置并发标记
            _context.Entry(user).Property("RowVersion").OriginalValue = updatedUser.RowVersion;

            try {
                _context.User.Update(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID {UserId} updated successfully", updatedUser.Id);
                return Ok("User updated successfully.");

            } catch (DbUpdateConcurrencyException) {
                _logger.LogWarning("使用ID更新用户时发生并发冲突 {UserId}", updatedUser.Id);
                return Conflict("Update failed due to concurrent changes.");

            } catch (DbUpdateException ex) {
                _logger.LogError(ex, "Error updating user with ID {UserId}", updatedUser.Id);
                return StatusCode(500, "Error updating user.");

            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error while updating user with ID {UserId}", updatedUser.Id);
                return StatusCode(500, "Unexpected error updating user.");
            }
        }

        /// <summary>
        /// 查询用户
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="name"></param>
        /// <param name="nickname"></param>
        /// <returns></returns>
        public async Task<List<User>> SearchUsers(string? phone = null, string? name = null, string? nickname = null) {
            _logger.LogInformation("Starting search with filters - Phone: {Phone}, Name: {Name}, Nickname: {Nickname}", phone, name, nickname);

            // 构建查询的基础对象
            var query = _context.User.AsQueryable();

            // 根据传入的参数动态添加条件
            if (!string.IsNullOrEmpty(phone)) {
                query = query.Where(u => u.Phone.Contains(phone));
            }
            if (!string.IsNullOrEmpty(name)) {
                query = query.Where(u => u.Name.Contains(name));
            }
            if (!string.IsNullOrEmpty(nickname)) {
                query = query.Where(u => u.Nickname.Contains(nickname));
            }

            var results = await query.ToListAsync();
            _logger.LogInformation("Found {Count} users with given filters", results.Count);

            return results;
        }

        public async Task<List<User>> SearchUsers(string? phone = null, string? name = null, string? nickname = null, string sortField = "Id", string sortOrder = "asc") {
            _logger.LogInformation("Starting search with filters - Phone: {Phone}, Name: {Name}, Nickname: {Nickname}, SortField: {SortField}, SortOrder: {SortOrder}",
                                   phone, name, nickname, sortField, sortOrder);

            var query = _context.User.AsQueryable();

            // 过滤条件
            if (!string.IsNullOrEmpty(phone)) {
                query = query.Where(u => u.Phone.Contains(phone));
            }
            if (!string.IsNullOrEmpty(name)) {
                query = query.Where(u => u.Name.Contains(name));
            }
            if (!string.IsNullOrEmpty(nickname)) {
                query = query.Where(u => u.Nickname.Contains(nickname));
            }

            // 排序逻辑
            query = sortField.ToLower() switch {
                "phone" => sortOrder == "asc" ? query.OrderBy(u => u.Phone) : query.OrderByDescending(u => u.Phone),
                "name" => sortOrder == "asc" ? query.OrderBy(u => u.Name) : query.OrderByDescending(u => u.Name),
                "createdat" => sortOrder == "asc" ? query.OrderBy(u => u.CreatedAt) : query.OrderByDescending(u => u.CreatedAt),
                _ => sortOrder == "asc" ? query.OrderBy(u => u.Id) : query.OrderByDescending(u => u.Id),
            };

            var results = await query.ToListAsync();
            _logger.LogInformation("Found {Count} users with given filters and sorting", results.Count);

            return results;
        }

        // 上传身份证图片
        public async Task<IActionResult> UploadImage(string flieName) {

            // 设置微信小程序的AppId和AppSecret
            var appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
            var appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;


            var envId = _wxSetting.Value.Env; // 用实际的环境ID替换这里的字符串

            string filePath = @"/admin/user/identityCardPictures" + flieName; // 用户身份证文件路径

            //var result = TcbApi.DatabaseCollectionGet(appId, envId);
            WxUploadFileJsonResult result;

            try {
                // 调用UploadFileAsync方法上传文件
                result = await TcbApi.UploadFileAsync(appId, envId, filePath);

                // 输出上传结果
                Console.WriteLine("File uploaded successfully!");
                Console.WriteLine("File ID: " + result.file_id);
            } catch (Exception ex) {
                // 输出错误信息
                Console.WriteLine("Error uploading file: " + ex.Message);
                return StatusCode(200);
            }



            string url = result.url;
            string key = filePath;
            string signature = result.authorization;// 签名
            string securityToken = result.token;
            string cosMetaFileId = result.cos_file_id;
            
            try {
                using (var client = new HttpClient())
                using (var formData = new MultipartFormDataContent()) {
                    // 添加 key 字段
                    formData.Add(new StringContent(key), "key");

                    // 添加 Signature 字段
                    formData.Add(new StringContent(signature), "Signature");

                    // 添加 x-cos-security-token 字段
                    formData.Add(new StringContent(securityToken), "x-cos-security-token");

                    // 添加 x-cos-meta-fileid 字段
                    formData.Add(new StringContent(cosMetaFileId), "x-cos-meta-fileid");

                    // 添加文件内容
                    //var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
                    //fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                    byte[] fileData = { };
                    //formData.Add(fileContent, "file", Path.GetFileName(filePath));
                    formData.Add(new ByteArrayContent(fileData), "file");

                    // 发送 POST 请求
                    var response = await client.PostAsync(url, formData);

                    Console.WriteLine("上传成功！");
                }
            } catch (Exception ex) {
                Console.WriteLine($"发生错误: {ex.Message}");
            }

            return StatusCode(200);
        }
    }
}
