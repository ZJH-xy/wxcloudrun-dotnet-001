namespace aspnetapp.Controllers {
    public class StoreController : IStoreRepository {

        private readonly MyDbContext _context;

        public StoreController(MyDbContext context) {
            _context = context;
        }

        public async Task<List<Store>> GetAllStore() {
            return await _context.Store.Where(s => s.IsDelete == false).ToListAsync();
        }

        public async Task<Store?> GetStoreById(int id) {
            return await _context.Store.SingleOrDefaultAsync(s => s.StoreId == id && s.IsDelete == false);
        }
    }
}
