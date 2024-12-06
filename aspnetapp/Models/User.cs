namespace aspnetapp.Models {
    /// <summary>
    /// 用户信息表
    /// </summary>
    [Table("T_Users")]
    [Index(nameof(Phone), IsUnique = true)]
    public class User {
		/// <summary>
		/// 用户编号
		/// </summary>
		[Key]
        public int Id { get; init; }

		/// <summary>
		/// 用户在开放平台的唯一标识符，若当前小程序已绑定到微信开放平台账号下会返回，详见 UnionID 机制说明。
		/// </summary>
		//public string Unionid { get; init; }

		/// <summary>
		/// 手机号码
		/// 使用 Data Annotations 进行格式验证
		/// </summary>
		[Required(ErrorMessage = "手机号码不可为空")]
        public string Phone { get; set; } = String.Empty;

		/// <summary>
		/// 密码
		/// </summary>
		public string? Password { get; set; }

		/// <summary>
		/// 姓名
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// 身份证号
		/// </summary>
		public string? IdentityCard {  get; set; }

		/// <summary>
		/// 身份证照片
		/// </summary>
		public string? IdentityCardPictures { get; set; }

		/// <summary>
		/// 昵称
		/// </summary>
		public string? Nickname { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreatedAt { get; set; }

		/// <summary>
		/// 更新时间
		/// </summary>
		public DateTime UpdatedAt { get; set; }

		/// <summary>
		/// 用于乐观并发控制
		/// </summary>
		[Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
