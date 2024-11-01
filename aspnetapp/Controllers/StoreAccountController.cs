namespace aspnetapp.Controllers {
    public class StoreAccountController : IStoreAccountRepository {

        private readonly MyDbContext _context;

        public StoreAccountController(MyDbContext context) {
            _context = context;
        }

        public async Task<Order?> GetConfirmOder(int orderId) {
            return await _context.Order.FindAsync(orderId);
        }

        public async Task<StoreAccount?> GetStoreAccount(string account) {
            return await _context.StoreAccount.SingleOrDefaultAsync(x => x.Account == account);
        }
    }
}
