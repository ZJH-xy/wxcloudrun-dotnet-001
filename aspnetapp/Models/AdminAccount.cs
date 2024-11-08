namespace aspnetapp.Models {
    /// <summary>
    /// 管理员帐号表
    /// </summary>
    [Table("T_AdminAccount")]
    public class AdminAccount {
        [Key]
        public int Id { get; init; }

        public string Account { get; set; }// 帐号

        public string Password { get; set; }// 密码
        
        [Timestamp]
        public byte[] RowVersion { get; set; }// 用于乐观并发控制
    }
}
