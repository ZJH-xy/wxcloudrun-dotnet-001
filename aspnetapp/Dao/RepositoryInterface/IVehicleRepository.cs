namespace aspnetapp.Dao.RepositoryInterface {
    public interface IVehicleRepository {
        /// <summary>
        /// 添加车辆
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> AddVehicle(Vehicle vehicle);
        /// <summary>
        /// 根据ID 查询车辆
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Vehicle?> GetVehicleById(int id);
        /// <summary>
        /// 批量获取车辆
        /// </summary>
        /// <param name="iimit">用于分页控制，表示返回多少行数据</param>
        /// <param name="offset">表示跳过前面多少条记录（即从哪一条记录开始）</param>
        /// <param name="includeStructure">是否返回表结构</param>
        /// <param name="includeData">是否返回表数据</param>
        /// <returns></returns>
        public IQueryable<Vehicle?> GetVehicles(int iimit, int offset, bool includeStructure = true, bool includeData = true);
        /// <summary>
        /// 更新车辆状态
        /// </summary>
        /// <param name="vehicleId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<int> ChangeVehicleStatus(int vehicleId, Vehicle.EVehicle status);
        /// <summary>
        /// 更新车辆
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> UpdateVehicle(Vehicle vehicle);
        /// <summary>
        /// 删除车辆
        /// </summary>
        /// <param name="id"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> DeleteVehicle(int id);
    }
}
