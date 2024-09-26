namespace aspnetapp.Dao.RepositoryInterface.Miniprogram
{
    public interface IUserRepository
    {
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> AddUser(User user);
        /// <summary>
        /// 根据ID 查询用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<User?> GetUserById(int id);
        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> UpdateUser(User user);

        // 更新密码
    }
}
