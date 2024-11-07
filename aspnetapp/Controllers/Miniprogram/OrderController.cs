namespace aspnetapp.Controllers.Miniprogram {
    public class OrderController : IOrderRepository {

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

        public async Task<Order.OrderStatus?> GetOrderStatusById(int userId, int orderid) {
            Order? order = await _context.Order.Where(o => o.TheUser == userId).FirstOrDefaultAsync(o => o.Id == orderid);
            return order?.Status;
        }
        public async Task<bool> GetOderReplacementByUserId(int userId, int orderId) {
            bool b = await _context.VehicleReplacementRecord.Where(vrr => vrr.TheOrder == orderId && vrr.State == VehicleReplacementRecord.Estates.侍确认).AnyAsync();
            Order? order = await _context.Order.FirstOrDefaultAsync(o => o.Id == orderId && o.TheUser == userId);
            return order is not null && b;
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
