using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace aspnetapp.Controllers.API.Background {
	/// <summary>
	/// 解密工具类
	/// </summary>
	public class AesGcm {
		private static string ALGORITHM = "AES/GCM/NoPadding";
		private static int TAG_LENGTH_BIT = 128;
		private static int NONCE_LENGTH_BYTE = 12;
		private static string AES_KEY = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_APIv3Key;//换成你的API V3密钥

		public static string AesGcmDecrypt(string associatedData, string nonce, string ciphertext) {
			GcmBlockCipher gcmBlockCipher = new GcmBlockCipher(new AesEngine());
			AeadParameters aeadParameters = new AeadParameters(
				new KeyParameter(Encoding.UTF8.GetBytes(AES_KEY)),
				128,
				Encoding.UTF8.GetBytes(nonce),
				Encoding.UTF8.GetBytes(associatedData));
			gcmBlockCipher.Init(false, aeadParameters);

			byte[] data = Convert.FromBase64String(ciphertext);
			byte[] plaintext = new byte[gcmBlockCipher.GetOutputSize(data.Length)];
			int length = gcmBlockCipher.ProcessBytes(data, 0, data.Length, plaintext, 0);
			gcmBlockCipher.DoFinal(plaintext, length);
			return Encoding.UTF8.GetString(plaintext);
		}
	}
}
