namespace aspnetapp.Models {
    /// <summary>
    /// 商家帐号表
    /// </summary>
    [Table("T_StoreAccount")]
    [Index(nameof(Account), IsUnique = true)]
    public class StoreAccount {
		/// <summary>
		/// 商家Id
		/// </summary>
		[Key]
        public int Id { get; set; }

		/// <summary>
		/// 逻辑指向门店
		/// </summary>
		public int TheStore { get; set; }

		/// <summary>
		/// 帐号
		/// </summary>
		public string Account { get; set; }

		/// <summary>
		/// 密码
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// 用于乐观并发控制
		/// </summary>
		[Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
