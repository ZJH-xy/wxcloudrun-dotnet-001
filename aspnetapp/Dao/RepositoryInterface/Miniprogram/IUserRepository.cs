namespace aspnetapp.Dao.RepositoryInterface.Miniprogram {
    public interface IUserRepository {
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> AddUser(User user);
        /// <summary>
        /// 根据phone 查询用户
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        Task<Object?> GetUserByPhone(int phone);
        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> UpdateUser(User user);

        // 更新密码
    }
}
