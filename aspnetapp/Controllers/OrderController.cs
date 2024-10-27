namespace aspnetapp.Controllers {
    public class OrderController :IOrderRepository {

        private readonly MyDbContext _context;

        public OrderController(MyDbContext context) {
            _context = context;
        }

        public async Task<int> AddOrder(Order order) {
            await _context.Order.AddAsync(order);
            return await _context.SaveChangesAsync();
        }

        public async  Task<Order?> GetById(int id) {
            return await _context.Order.SingleOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<List<Order>> GetOrderByUserId(int userId) {
            List<Order> oderList = await _context.Order.Where(o => o.TheUser == userId).Take(10).ToListAsync();// 获取20条
            return oderList;
        }

        public async Task<int> UpdateOrder(Order order) {
            order.UpdatedAt = DateTime.Now;
            _context.Order.Update(order);
            return await _context.SaveChangesAsync();
        }
    }
}
