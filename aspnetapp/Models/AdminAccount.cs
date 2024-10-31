namespace aspnetapp.Models {
    /// <summary>
    /// 管理员帐号表
    /// </summary>
    [Table("T_AdminAccount")]
    public class AdminAccount {
        [Key]
        public int Id { get; set; }

        public string Account { get; set; }// 帐号

        public string Password { get; set; }// 密码
    }
}
