namespace aspnetapp.Models {
    public class Vehicles {
        private int _vehicleId;
        private Estates _state;
        private string? _type = null;

        public int VehicleId {
            set => _vehicleId = value;
            get => _vehicleId; 
        }

        public string? Type {
            set => _type = value;
            get => _type;
        }

        public Estates State {
            set {
                if (Estates.IsDefined(value)) {
                    _state = value;
                } else {
                    throw new Exception("Estates 状态值无效");
                }
            }
            get {
                return _state;
            }
        }

        public enum Estates {
            Idle,// 空闲
            Leased,// 已出租
            Charging,// 充电中
            Fault// 故障
        }
    }
}
