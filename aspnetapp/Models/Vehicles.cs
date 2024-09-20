namespace aspnetapp.Models {
    public class Vehicles {
        private int _vehicleId;
        private States _state;
        private string? _type = null;

        public int VehicleId {
            set => _vehicleId = value;
            get => _vehicleId; 
        }
        public string? Type {
            set => _type = value;
            get => _type;
        }
        public States State {
            get {
                return _state;
            }
            set {
                if (States.IsDefined(value)) {
                    _state = value;
                } else {
                    throw new Exception("State 状态值无效");
                }
            }
        }

        public enum States {
            IDLE,// 空闲
            LEASED,// 已出租
            CHARGING,// 充电中
            FAULT// 故障
        }
    }
}
