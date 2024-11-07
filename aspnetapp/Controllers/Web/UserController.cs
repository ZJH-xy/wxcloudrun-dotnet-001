using aspnetapp.Dao.RepositoryInterface.Web;

namespace aspnetapp.Controllers.Web {
    public class UserController : IUserRepositoryWeb {
        public Task<object?> GetById(int id) {
            throw new NotImplementedException();
        }

        public Task<List<object>> GetTablePage(int iimit, int pageIndex) {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetTableStructure() {
            throw new NotImplementedException();
        }
    }
}
