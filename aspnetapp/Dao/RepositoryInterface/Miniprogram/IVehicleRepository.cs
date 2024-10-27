namespace aspnetapp.Dao.RepositoryInterface.Miniprogram {
    public interface IVehicleRepository {
        /// <summary>
        /// 根据门店StoreId 查询车辆
        /// </summary>
        /// <param name="storeId"></param>
        /// <returns></returns>
        Task<List<Vehicle>> GetVehicleByStoreId(int storeId);
        // 
        Task<Vehicle?> GetVehicleById(int vehicleId);
        // 查询车辆模型
        Task<Object?> GetVehicleQueryableById(int vehicleId);
    }
}
