namespace aspnetapp.Controllers.API.Background {
    public class Judge {
        /// <summary>
        /// 姓名格式判断
        /// </summary>
        /// <param name="name"></param>
        /// <returns>符合为true</returns>
        public static bool NameFormatDetermination(string name) {
            return Regex.IsMatch(name, @"^([\\u4e00-\\u9fa5]{1,20}|[a-zA-Z\\.\\s]{1,20})$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 身份证格式判断
        /// </summary>
        /// <param name="identityCard"></param>
        /// <returns>符合为true</returns>
        public static bool IdentityCardFormatDetermination(string identityCard) {
            return Regex.IsMatch(identityCard, @"^(^\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 手机号格式判断
        /// </summary>
        /// <param name="phone"></param>
        /// <returns>符合为true</returns>
        public static bool PhoneFormatDetermination(string phone) {
            return Regex.IsMatch(phone, @"^1(3[0-9]|4[01456879]|5[0-35-9]|6[2567]|7[0-8]|8[0-9]|9[0-35-9])\d{8}$");
        }

        /// <summary>
        /// 密码格式判断
        /// </summary>
        /// <param name="password"></param>
        /// <returns>符合为true</returns>
        public static bool PasswordFormatDetermination(string password) {
            return true;
            //if (password.Length < 8 ||  password.Length > 12)
            //    return false;

            //return true;
        }
    }
}
