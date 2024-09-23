using aspnetapp.Models;

namespace aspnetapp.Dao.ContextBases {
    public abstract class UserRepositoryBase {
        public abstract void AddUser(User user);
        public abstract User? GetUserById(int id);
        public abstract IEnumerable<User> GetAllUsers();
        public abstract void UpdateUser(User user);
        public abstract void DeleteUser(int id);

        // 修改密码
    }
}
