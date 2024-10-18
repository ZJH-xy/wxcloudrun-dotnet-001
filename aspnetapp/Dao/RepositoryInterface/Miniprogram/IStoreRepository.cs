namespace aspnetapp.Dao.RepositoryInterface.Miniprogram {
    public interface IStoreRepository {
        /// <summary>
        /// 获取所有门店基础信息
        /// </summary>
        /// <returns></returns>
        Task<List<StoreBasic>> GetAllOrders();
        //
        Task<Store?> GetStoreById(int id);
    }
}
