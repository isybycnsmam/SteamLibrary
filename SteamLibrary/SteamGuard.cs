using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace SteamLibrary {
	/// <summary>
	/// Use this class to generate two factor codes
	/// </summary>
	public static class SteamGuard {

		private static HttpClient client = new HttpClient();

		private static byte[] steamGuardCodeTranslations = new byte[] {
			50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71,
			72, 74, 75, 77,78, 80, 81, 82, 84, 86, 87, 88, 89 };


		/// <summary>
		/// Method that generates two factor code from secret with can be found in steam mobile app files
		/// </summary>
		/// <exception cref="ArgumentException">Secret is not valid</exception>
		/// <exception cref="HttpRequestException">Error in sending requests, contains inner exception</exception>
		/// <exception cref="CryptographicException">Cryptobraphic transactions failed</exception>
		/// <param name="secret">Token from mobile app</param>
		/// <returns>Guard code</returns>
		public static string GenerateTwoFactorCode(string secret) {

			if (secret.Length != 28)
				throw new ArgumentException("secret is not valid");

			var serverTime = getInfoFromServer(secret);

			return generateSteamGuardCodeForTime(secret, serverTime);

		}

		private static long getInfoFromServer(string secret) {

			var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com:443/ITwoFactorService/QueryTime/v0001");
			var response = client.SendAsync(request).Result.EnsureSuccessStatusCode();
			var responseString = response.Content.ReadAsStringAsync().Result;
			dynamic stuff = JsonConvert.DeserializeObject(responseString);

			return stuff.response.server_time ?? throw new HttpRequestException("server_time server time is empty");

		}

		private static string generateSteamGuardCodeForTime(string secret, long time) {

			try {

				byte[] sharedSecretArray = Convert.FromBase64String(Regex.Unescape(secret));
				byte[] timeArray = new byte[8];

				time /= 30L;

				for (int i = 8; i > 0; i--) {
					timeArray[i - 1] = (byte)time;
					time >>= 8;
				}


				HMACSHA1 hmacGenerator = new HMACSHA1();
				hmacGenerator.Key = sharedSecretArray;
				byte[] hashedData = hmacGenerator.ComputeHash(timeArray);
				byte[] codeArray = new byte[5];

				byte b = (byte)(hashedData[19] & 0xF);
				int codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16 | (hashedData[b + 2] & 0xFF) << 8 | (hashedData[b + 3] & 0xFF);

				for (int i = 0; i < 5; ++i) {
					codeArray[i] = steamGuardCodeTranslations[codePoint % steamGuardCodeTranslations.Length];
					codePoint /= steamGuardCodeTranslations.Length;
				}

				return Encoding.UTF8.GetString(codeArray);

			}
			catch (Exception ex) { throw new CryptographicException("generating two factor code failed", ex); }

		}

	}
}