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
            get {
                return _state;
            }
            set {
                if (Estates.IsDefined(value)) {
                    _state = value;
                } else {
                    throw new Exception("State 状态值无效");
                }
            }
        }

        public enum Estates {
            IDLE,// 空闲
            LEASED,// 已出租
            CHARGING,// 充电中
            FAULT// 故障
        }
    }
}
