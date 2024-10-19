namespace aspnetapp.Controllers {
    public class VehicleController : IVehicleRepository {
        private readonly MyDbContext _context;
        public VehicleController(MyDbContext context) {
            _context = context;
        }

        public async Task<List<VehicleBasic>> GetVehicleByStoreId(int storeId) {
            List<Vehicle> vehicleList = await _context.Vehicle.Where(s => s.IsDelete == false && s.CurrentStore == storeId).ToListAsync();
            List<VehicleBasic> vehiclesBasicsList = new List<VehicleBasic>();

            foreach (Vehicle vehicle in vehicleList) {
                vehiclesBasicsList.Add(new VehicleBasic(vehicle));
            }
            return vehiclesBasicsList;
        }
    }

    public struct VehicleBasic {
        public VehicleBasic(Vehicle store) {
            VehicleId = store.VehicleId;
            CurrentStore = store.CurrentStore;
            Model = store.Model;
            PlateNumber = store.PlateNumber;
            FrameNumber = store.FrameNumber;
            VehicleIntroduction = store.VehicleIntroduction;
            State = store.State;
        }
        public int VehicleId { get; init; }// 车辆编号

        [JsonConverter(typeof(StringEnumConverter), true)]
        public Vehicle.Estates State { get; set; }// 车辆状态

        [JsonConverter(typeof(StringEnumConverter), true)]
        public Vehicle.Emodel Model { get; set; } = Vehicle.Emodel.UnknownOrSecondHand;// 车辆型号
        public int? CurrentStore { get; set; }// 当前门店
        public string? PlateNumber { get; set; }// 车牌牌号
        public string? FrameNumber { get; set; }// 车架号
        public string? VehicleIntroduction { get; set; }// 车辆介绍
    }
}
