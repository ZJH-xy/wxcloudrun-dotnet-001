namespace aspnetapp.Controllers.API.Background {
    public class Judge {
		/// <summary>
		/// 中国汉字姓名格式验证（支持少数民族姓名间隔符）
		/// </summary>
		/// <param name="name">待验证的姓名</param>
		/// <returns>
		/// 验证结果：
		/// true - 符合规范（2-15个中文字符，允许使用间隔符·，如：阿沛·阿旺晋美）
		/// false - 不符合规范
		/// </returns>
		public static bool NameFormatDetermination(string name) {
			/*
			 *  在调用验证方法前建议做标准化处理
				public static string NormalizeName(string input)
				{
					return input?
						.Trim()                         // 去除首尾空格
						.Replace(" ", "")              // 去除中间空格
						.Replace("\u3000", "")         // 去除全角空格
						.Replace("•", "·")             // 统一间隔符格式
						.Replace("‧", "·")             // 兼容不同编码的间隔符
						.Normalize(NormalizationForm.FormKC);
				}

				// 使用示例
				var normalizedName = NormalizeName(rawName);
				if(!NameFormatDetermination(normalizedName))
				{
					throw new ArgumentException("姓名格式不符合规范");
				}
			 */

			// 空值检查
			if (string.IsNullOrWhiteSpace(name)) {
				return false;
			}

			/* 正则表达式说明：
			 * ^[\u4e00-\u9fa5]  以中文开头
			 * (·?[\u4e00-\u9fa5]+)* 允许中间出现间隔符+中文的组合
			 * {2,15}$ 总长度2-15个字符（中文和间隔符合计）
			 * 
			 * 匹配示例：
			 * 正确：张三、欧阳修、迪丽热巴·迪力木拉提
			 * 错误：A三、张@三、·张三、张·三·、纯数字、过长姓名
			 */
			return Regex.IsMatch(name,
				@"^                   # 开头
				[\u4e00-\u9fa5]      # 首个字符必须是中文
				(                    # 组合开始
				  ·?                 # 可选间隔符（不能连续出现）
				  [\u4e00-\u9fa5]+   # 必须跟随中文
				)*                   # 组合可重复
				$                    # 结尾
				",
				RegexOptions.IgnorePatternWhitespace)
				&& name.Length >= 2
				&& name.Length <= 15;
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
