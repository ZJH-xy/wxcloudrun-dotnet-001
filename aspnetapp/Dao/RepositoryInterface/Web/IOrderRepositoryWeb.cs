namespace aspnetapp.Dao.RepositoryInterface.Web {
    public interface IOrderRepositoryWeb : IOrderRepository {
        /// <summary>
        /// 根据OrderId 查询订单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Order?> GetOrderById(int id);
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
        //Task<int> DeleteOrder(int id);
    }
}
