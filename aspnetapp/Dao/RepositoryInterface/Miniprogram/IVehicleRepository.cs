namespace aspnetapp.Dao.RepositoryInterface.Miniprogram
{
    public interface IVehicleRepository
    {
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
    }
}
