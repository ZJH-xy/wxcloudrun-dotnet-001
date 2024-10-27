namespace aspnetapp.Controllers {
    public class VehicleController : IVehicleRepository {

        private readonly MyDbContext _context;

        public VehicleController(MyDbContext context) {
            _context = context;
        }

        public async Task<Vehicle?> GetVehicleById(int vehicleId) {
            return await _context.Vehicle.SingleOrDefaultAsync(v => v.IsDelete == false && v.VehicleId == vehicleId);
        }

        public async Task<List<Vehicle>> GetVehicleByStoreId(int storeId) {
            return await _context.Vehicle.Where(s => s.IsDelete == false && s.CurrentStore == storeId).ToListAsync();
        }

        public async Task<Object?> GetVehicleQueryableById(int vehicleId) {
            return await _context.Vehicle.Where(v => v.VehicleId.Equals(vehicleId) && ! v.IsDelete).Select(v => new { v.Model, v.PlateNumber }).ToArrayAsync();
        }
    }
}
