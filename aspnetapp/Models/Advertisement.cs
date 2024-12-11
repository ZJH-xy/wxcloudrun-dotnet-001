namespace aspnetapp.Models {
    /// <summary>
    /// 广告表
    /// </summary>
    [Table("T_Advertisement")]
    [Index(nameof(IsDelete))]
    public class Advertisement {
        /// <summary>
        /// 广告表ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 跳转链接
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; } = false;

        /// <summary>
        /// 用于乐观并发控制
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
