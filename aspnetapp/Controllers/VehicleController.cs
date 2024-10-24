namespace aspnetapp.Controllers {
    public class VehicleController : IVehicleRepository {

        private readonly MyDbContext _context;

        public VehicleController(MyDbContext context) {
            _context = context;
        }

        public async Task<VehiclePro?> GetVehicleById(int vehicleId) {
            Vehicle? vehicle = await _context.Vehicle.SingleOrDefaultAsync(v => v.VehicleId == vehicleId);
            return vehicle is null ? null : new VehiclePro(vehicle);
        }

        public async Task<List<VehicleBasic>> GetVehicleByStoreId(int storeId) {
            List<Vehicle> vehicleList = await _context.Vehicle.Where(s => s.IsDelete == false && s.CurrentStore == storeId).ToListAsync();
            List<VehicleBasic> vehiclesBasicsList = new();

            foreach (Vehicle vehicle in vehicleList) {
                vehiclesBasicsList.Add(new VehicleBasic(vehicle));
            }
            return vehiclesBasicsList;
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
