namespace aspnetapp.Dao.RepositoryInterface.Web {
    public interface IUserRepositoryWeb : IUserRepository {
        /// <summary>
        /// 根据ID 查询用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<User?> GetUserById(int id);
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> DeleteUser(int id);
    }
}
