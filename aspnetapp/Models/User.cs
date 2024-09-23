namespace aspnetapp.Models {
    public class User {
        private int _userId;
        private string _phone = String.Empty;
        private string? _password;
        private string? _name;
        private DateTime _createdAt;
        private DateTime _updatedAt;

        public int UserId {
            set => _userId = value;
            get => _userId;
        }

        public string Phone {
            set {
                if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^(\d{3,4}-)?\d{6,8}$"))// 号码判断
                    throw new Exception("手机号格式无效");
                if (value is null ||value == String.Empty)
                    throw new ArgumentNullException(nameof(value));
                _phone = value;
            }
            get => _phone;
        }

        public string? Name {
            set {
                if (value is null || value == String.Empty)
                    throw new ArgumentNullException(nameof(value));
                else
                    _name = value;
            }
            get => _name;
        }

        public string? Password {
            set {
                if (value is null || value == String.Empty)
                    throw new ArgumentNullException(nameof(value));
                else
                    _password = value;
            }
            get => _password;
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
