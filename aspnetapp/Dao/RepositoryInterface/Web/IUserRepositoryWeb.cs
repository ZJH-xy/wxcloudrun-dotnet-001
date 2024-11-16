namespace aspnetapp.Dao.RepositoryInterface.Web {
    public interface IUserRepositoryWeb : IRepositoryWeb {
        /// <summary>
        /// 根据ID 查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<User?> GetById(int id);
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="iimit">一页多少行</param>
        /// <param name="pageIndex">第几页</param>
        /// <returns></returns>
        Task<List<User>> GetTablePage(int iimit, int pageIndex);
        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="updatedUser"></param>
        /// <returns></returns>
        IActionResult UpdateUser(User updatedUser);
        /// <summary>
        /// 根据ID 查询用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //Task<User?> GetById(int id);
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns>写入数据库的状态条目数</returns>
        //Task<int> DeleteById(int id);
    }
}
