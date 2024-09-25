namespace aspnetapp.Dao.RepositoryInterface {
    public interface IOrderRepository {
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
        /// <summary>
        /// 批量获取订单
        /// </summary>
        /// <param name="iimit">用于分页控制，表示返回多少行数据</param>
        /// <param name="offset">表示跳过前面多少条记录（即从哪一条记录开始）</param>
        /// <param name="includeStructure">是否返回表结构</param>
        /// <param name="includeData">是否返回表数据</param>
        /// <returns></returns>
        public IQueryable<Order?> GetOrders(int iimit, int offset, bool includeStructure = true, bool includeData = true);
        /// <summary>
        /// 更改订单状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task<int> ChangeOrderStatus(int orderId, Order.OrderStatus status);

        /// <summary>
        /// 更新订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> UpdateOrder(Order order);
        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> DeleteOrder(int id);
    }
}
