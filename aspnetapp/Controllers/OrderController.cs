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

        public async Task<Order?> GetById(int userId, int orderid) {
            return await _context.Order.Where(o => o.TheUser == userId).FirstOrDefaultAsync(o => o.Id == orderid);
        }

        public async Task<List<Order>> GetOrderByUserId(int userId) {
            return await _context.Order.Where(o => o.TheUser == userId).OrderByDescending(o => o.CreatedAt).Take(10).ToListAsync();// 获取10条
        }

        public async Task<int> UpdateOrder(Order order) {
            order.UpdatedAt = DateTime.Now;
            _context.Order.Update(order);
            return await _context.SaveChangesAsync();
        }
    }
}
