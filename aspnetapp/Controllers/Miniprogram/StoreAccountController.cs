namespace aspnetapp.Controllers.Miniprogram {
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

		// 根据车牌号查找未删除的车辆
		public async Task<Vehicle?> GetVehicleByPlateNumber(string plateNumber) {
			return await _context.Vehicle.FirstOrDefaultAsync(v => !v.IsDelete && v.PlateNumber == plateNumber);
		}
	}
}
