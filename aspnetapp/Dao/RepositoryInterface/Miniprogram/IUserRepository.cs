using static aspnetapp.Controllers.Miniprogram.UserController;

namespace aspnetapp.Dao.RepositoryInterface.Miniprogram
{
    public interface IUserRepository {
        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> AddUser(User user);
        /// <summary>
        /// 获取用户所有信息
        /// </summary>
        /// <param name="id"></param>
        /// 
        /// <returns>用户所有信息</returns>
        Task<User?> GetUser(int id);
        /// <summary>
        /// 根据id 查询用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns>用户昵称等基本信息</returns>
        Task<User?> GetUserById(int id);
        /// <summary>
        /// 根据phone 查询用户
        /// </summary>
        /// <param name="phone"></param>
        /// <returns>用户昵称等基本信息</returns>
        Task<User?> GetUserByPhone(string phone);
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> DeleteUserById(int id);
        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="user"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> UpdateUser(User user);

        // 更新密码
    }
}
