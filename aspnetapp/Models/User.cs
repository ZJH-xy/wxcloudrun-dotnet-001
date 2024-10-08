namespace aspnetapp.Models {
    // 用户信息表
    [Table("T_Users")]
    public class User {
        [Key]
        public int UserId { get; init; }// 用户编号

        // 使用 Data Annotations 进行格式验证
        //[RegularExpression(@"^1(3[0-9]|4[01456879]|5[0-35-9]|6[2567]|7[0-8]|8[0-9]|9[0-35-9])\d{8}$", ErrorMessage = "手机号格式无效
        [Required(ErrorMessage = "手机号码不可为空")]

        public string Phone { get; set; } = String.Empty;// 手机号码

        //[DataType(DataType.Password)]
        public string? Password { get; set; }// 密码

        public string? Name { get; set; }// 姓名

        public string? IdentityCard {  get; set; }// 身份证号

        public string? Nickname { get; set; }// 昵称

        public ICollection<Store> FavoriteStores { get; set; } = new List<Store>();// 收藏门店

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
