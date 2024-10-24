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
    }

    public struct VehicleBasic {
        public VehicleBasic(Vehicle store) {
            VehicleId = store.VehicleId;
            Model = store.Model;
            State = store.State;
        }

        public int VehicleId { get; init; }// 车辆编号

        [JsonConverter(typeof(StringEnumConverter), true)]/* 枚举类型使用其名称而不是int */
        public Vehicle.Estates State { get; set; }// 车辆状态

        [JsonConverter(typeof(StringEnumConverter), true)]/* 枚举类型使用其名称而不是int */
        public Vehicle.Emodel Model { get; set; } = Vehicle.Emodel.未知;// 车辆型号
    }

    public struct VehiclePro {
        public VehiclePro(Vehicle store) {
            VehicleId = store.VehicleId;
            Model = store.Model;
            PlateNumber = store.PlateNumber;
            FrameNumber = store.FrameNumber;
            VehicleIntroduction = store.VehicleIntroduction;
            State = store.State;
        }

        public int VehicleId { get; init; }// 车辆编号

        [JsonConverter(typeof(StringEnumConverter), true)]/* 枚举类型使用其名称而不是int */
        public Vehicle.Estates State { get; set; }// 车辆状态

        [JsonConverter(typeof(StringEnumConverter), true)]/* 枚举类型使用其名称而不是int */
        public Vehicle.Emodel Model { get; set; } = Vehicle.Emodel.未知;// 车辆型号

        public string? PlateNumber { get; set; }// 车牌牌号

        public string? FrameNumber { get; set; }// 车架号

        public string? VehicleIntroduction { get; set; }// 车辆介绍
    }
}
