namespace aspnetapp.Dao.RepositoryInterface.Miniprogram {
    public interface IStoreAccountRepository {
        /// <summary>
        /// 获取单个商家
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        Task<StoreAccount?> GetStoreAccount(string account);
        /// <summary>
        /// 商家获取单个订单
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<Order?> GetConfirmOder(int orderId);
    }
}
