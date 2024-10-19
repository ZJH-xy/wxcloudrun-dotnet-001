namespace aspnetapp.Dao.RepositoryInterface.Miniprogram {
    public interface IVehicleRepository {
        /// <summary>
        /// 根据门店StoreId 查询车辆
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        Task<List<VehicleBasic>> GetVehicleByStoreId(int storeId);
    }
}
