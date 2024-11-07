namespace aspnetapp.Dao.RepositoryInterface.Web {
    public interface IUserRepositoryWeb : IRepositoryWeb {
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
