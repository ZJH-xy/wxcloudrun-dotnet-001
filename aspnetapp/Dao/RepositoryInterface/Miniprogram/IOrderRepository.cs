namespace aspnetapp.Dao.RepositoryInterface.Miniprogram
{
    public interface IOrderRepository
    {
        /// <summary>
        /// 添加订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> AddOrder(Order order);
        /// <summary>
        /// 根据ID 查询订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Order?> GetOrderById(int id);
        /// <summary>
        /// 根据用户编号查询订单
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IQueryable<Order?>> GetOrderByName(int userId);
    }
}
