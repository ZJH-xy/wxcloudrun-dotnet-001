namespace aspnetapp.Models {
	/// <summary>
	/// 配置相关
	/// </summary>
	[Table("Z_Configuration")]
	public class Configuration {
		/// <summary>
		/// Id
		/// </summary>
		[Key]
		public int Id { get; init; }

		/// <summary>
		/// 索引
		/// </summary>
		public string? Key { get; set; }

		/// <summary>
		/// 值
		/// </summary>
		public string? Value { get; set; }
	}
}
