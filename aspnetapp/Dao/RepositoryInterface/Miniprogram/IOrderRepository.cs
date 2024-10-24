namespace aspnetapp.Dao.RepositoryInterface.Miniprogram {
    public interface IOrderRepository {
        // 获取订单
        Task<Order?> GetById(int id);
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
        /// 更新订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<int> UpdateOrder(Order order);
    }
}
