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

		/// <summary>
        /// 根据车牌号查找未删除的车辆
        /// </summary>
        /// <param name="plateNumber"></param>
        /// <returns></returns>
		public async Task<Vehicle?> GetVehicleByPlateNumber(string plateNumber) {
			return await _context.Vehicle.FirstOrDefaultAsync(v => !v.IsDelete && v.PlateNumber == plateNumber);
		}

		/// <summary>
		/// 根据商家ID获取对应门店ID
		/// </summary>
		/// <param name="storeAccountId">商家ID</param>
		/// <returns>对应门店ID</returns>
		public async Task<int> GetTheStoreById(int storeAccountId) {
			//return await _context.StoreAccount.Where(sa => sa.Id == storeId).Select(sa => sa.TheStore).FirstAsync();
			var sa = await _context.StoreAccount.SingleAsync(sa => sa.Id == storeAccountId);
            return sa.TheStore;
        }
	}
}
