namespace aspnetapp.Dao.RepositoryInterface.Miniprogram {
    public interface IStoreRepository {
        /// <summary>
        /// 获取所有门店基础信息
        /// </summary>
        /// <returns></returns>
        Task<List<Store>> GetAllStore();
        /// <summary>
        /// 根据id获取门店
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Store?> GetStoreById(int id);
    }
}
