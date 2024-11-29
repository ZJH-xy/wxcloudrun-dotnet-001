namespace aspnetapp.Dao.RepositoryInterface.Miniprogram {
    public interface IOrderRepository {
        /// <summary>
        /// 获取单个订单
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        Task<Order?> GetById(int userId, int orderid);
        /// <summary>
        /// 添加订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns>写入数据库的状态条目数</returns>
        Task<int> AddOrder(Order order);
        /// <summary>
        /// 根据用户UserId 查询订单
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<Order>> GetOrderByUserId(int userId);
        /// <summary>
        /// 获取订单状态
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        Task<Order.EOrderStatus?> GetOrderStatusById(int userId, int orderid);
        /// <summary>
        /// 获取订单换车状态
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="orderid"></param>
        /// <returns>正在换车为true，反之false</returns>
        Task<bool> GetOderReplacementByUserId(int userId, int orderid);
        /// <summary>
        /// 更新订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<int> UpdateOrder(Order order);
    }
}
