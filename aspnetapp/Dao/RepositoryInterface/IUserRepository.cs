using aspnetapp.Models;

namespace aspnetapp.Dao.ContextBases {
    public interface IUserRepository {
        // 添加用户
        Task AddUser(User user);
        // 根据ID 查询用户
        Task<User>? GetUserById(int id);
        // 获取所有用户
        IQueryable<User> GetAllUsers();
        // 更新用户
        Task UpdateUser(User user);
        // 删除用户
        Task DeleteUser(int id);

        // 修改密码
    }
}
