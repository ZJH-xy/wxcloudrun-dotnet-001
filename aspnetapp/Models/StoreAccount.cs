namespace aspnetapp.Models {
    /// <summary>
    /// 商家帐号表
    /// </summary>
    [Table("T_StoreAccount")]
    [Index(nameof(TheStore), nameof(Account), IsUnique = true)]
    public class StoreAccount {
        [Key]
        public int Id { get; init; }

        /* 逻辑指向门店 */
        public int TheStore { get; set; }

        public string Account { get; set; }// 帐号

        public string Password { get; set; }// 密码

        [Timestamp]
        public byte[] RowVersion { get; set; }// 用于乐观并发控制
    }
}
