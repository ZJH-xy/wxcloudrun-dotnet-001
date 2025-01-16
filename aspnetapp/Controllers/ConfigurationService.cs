using System;

namespace aspnetapp.Controllers {
	public class ConfigurationService {
		private readonly MyDbContext _dbContext;

		public ConfigurationService(MyDbContext context) {
			_dbContext = context;
		}

		/// <summary>
		/// 获取配置值
		/// </summary>
		/// <param name="key">配置项索引</param>
		/// <returns>配置值</returns>
		public async Task<string?> GetConfigValueAsync(string key) {
			var config = await _dbContext.Configurations
				.FirstOrDefaultAsync(c => c.Key == key);
			return config?.Value;
		}

		/// <summary>
		/// 获取调度费
		/// </summary>
		/// <returns>调度费(decimal)，如果配置不存在则返回默认值</returns>
		public async Task<decimal> GetDispatchFeeAsync() {
			var value = await GetConfigValueAsync("DispatchFee");
			return decimal.TryParse(value, out var fee) ? fee : 10; // 默认值为10
		}
	}
}
