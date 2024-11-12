
namespace aspnetapp.Controllers.Miniprogram {
    public class FavoritesStoreController : IFavoritesStoreRepository {

        private readonly MyDbContext _context;

        public FavoritesStoreController(MyDbContext context) {
            _context = context;
        }

        public async Task<int> AddFavoritesStore(int userId, int storeId) {
            UserFavoritesStore userFavoritesStore = new() {
                TheUser = userId,
                TheStore = storeId,
                CreatedAt = DateTime.Now
            };
            await _context.AddAsync(userFavoritesStore);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DelFavoritesStore(int favoritesStoreId) {
            var u = await _context.UserFavoritesStore.FirstOrDefaultAsync(u => u.Id == favoritesStoreId);
            if (u is null)
                return 0;
            _context.UserFavoritesStore.Remove(u);
            return await _context.SaveChangesAsync();
        }

        public async Task<UserFavoritesStore?> GetById(int id) {
            return await _context.UserFavoritesStore.FindAsync(id);
        }

        public async Task<List<UserFavoritesStore>> GetByUserId(int userId) {
            return await _context.UserFavoritesStore.Where(u => u.TheUser == userId).ToListAsync();
        }
    }
}
