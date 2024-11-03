namespace aspnetapp.Controllers {
    public class StoreAccountController : IStoreAccountRepository {

        private readonly MyDbContext _context;

        public StoreAccountController(MyDbContext context) {
            _context = context;
        }

        public async Task<Order?> GetConfirmOder(int orderId) {
            return await _context.Order.FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<StoreAccount?> GetStoreAccount(string account) {
            return await _context.StoreAccount.FirstOrDefaultAsync(x => x.Account == account);
        }
    }
}
