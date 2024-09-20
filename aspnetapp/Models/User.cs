namespace aspnetapp.Models {
    public class User {
        private int _userId;
        private string _phone;
        private string? _password;
        private string? _name;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int UserId {
            set => _userId = value;
            get => _userId;
        }
        public string Phone {
            // 号码判断
            set => _phone = value;
            get => _phone;
        }
        public string? Name {
            set => _name = value;
            get => _name;
        }
        public string? Password {
            set => _password = value;
        }
        public DateTime CreatedAt {
            set => _createdAt = value;
            get => _createdAt;
        }
        public DateTime UpdatedAt {
            set => _updatedAt = value;
            get => _updatedAt;
        }
    }
}
