namespace aspnetapp.Models {
    /// <summary>
    /// 管理员帐号表
    /// </summary>
    [Table("T_AdminAccount")]
    public class AdminAccount {
		/// <summary>
		/// 管理员编号
		/// </summary>
		[Key]
        public int Id { get; set; }

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
