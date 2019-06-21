using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using SteamLibrary.Exceptions;

namespace SteamLibrary {
	public partial class SteamClient {

		public SteamClient() { }

		/// <summary>
		/// Constructor with parrameter that sets cookies to HttpClient
		/// </summary>
		/// <param name="cookies">Cookies to add to HttpClient</param>
		public SteamClient(CookieCollection cookies) {

			httpPack = new HttpPack();

			foreach (Cookie cookie in cookies)
				httpPack.handler.CookieContainer.Add(cookie);

		}


		/// <summary>
		/// Method used to login to steam
		/// </summary>
		/// <exception cref="ArgumentNullException">Password or username is null or empty</exception>
		/// <exception cref="HttpRequestException">Error in sending requests, contains inner exception</exception>
		/// <exception cref="MyLoginException">Response json status is false, contains response json</exception>
		/// <exception cref="CryptographicException">Cryptobraphic transactions failed</exception>
		/// <exception cref="CaptchaNeededException">You must fill the captcha to login, may apperar when you are trying to login multiple times, contains captcha object that contains info about captcha that you should resend in next attemp</exception>
		/// <param name="user">Login to steam</param>
		/// <param name="password">Password to steam</param>
		/// <param name="authorization">Token to generate code or code if needed</param>
		/// <param name="remember">Remember login</param>
		/// <param name="captcha">Object that contains data about captcha id and text</param>
		public void Login(string user, string password, string authorization = "", bool remember = false, Captcha captcha = null) {

			if (captcha == null)
				captcha = new Captcha();

			if (string.IsNullOrEmpty(user))
				throw new ArgumentNullException("user value is null or empty");
			else if (string.IsNullOrEmpty(password))
				throw new ArgumentNullException("password value is null or empty");

			httpPack = new HttpPack();

			User = user;
			Password = password;
			if (authorization.Length == 28)
				Token = authorization;


			var ts = "";

			var encryptedPassword = getRSA(out ts, Password);
	
			sendLoginDataToSteam(encryptedPassword, remember, authorization, ts, captcha);

			submitCookies();

		}


		private string getRSA(out string ts, string password) {

			var mod = "";
			var exp = "";
			var responseString = "";
			dynamic stuff = "";


			try {

				var request = new HttpRequestMessage(HttpMethod.Post, "https://steamcommunity.com/login/getrsakey/");

				request.Content = new StringContent($"username={User}", Encoding.UTF8, "application/x-www-form-urlencoded");

				var x = httpPack.client.SendAsync(request).Result;

				if (!x.IsSuccessStatusCode)
					throw new HttpRequestException("x.IsSuccessStatusCode = false");

				responseString = x.Content.ReadAsStringAsync().Result;

				x?.Dispose();

				stuff = JsonConvert.DeserializeObject(responseString);

			}
			catch (Exception ex) { throw new HttpRequestException("getRSA", ex); }


			try {

				if (stuff.success != "true")
					throw new Exception();

				mod = stuff.publickey_mod;
				exp = stuff.publickey_exp;
				ts = stuff.timestamp;

				if (string.IsNullOrEmpty(mod) || string.IsNullOrEmpty(exp) || string.IsNullOrEmpty(ts))
					throw new Exception();

			}
			catch { throw new MyLoginException("getRSA", stuff ?? ""); }
			

			return encryptPassword(mod, exp, password);

		}

		private void sendLoginDataToSteam(string encryptedPassword, bool remember, string authorization, string ts, Captcha captcha) {

			var responseString = "";

			dynamic stuff = "";

			try {

				var request = new HttpRequestMessage(HttpMethod.Post, "https://steamcommunity.com/login/dologin/");

				var values = new Dictionary<string, string> {
					{"captcha_text", captcha.text},
					{"captchagid", captcha.gid},
					{"password", encryptedPassword},
					{"remember_login", remember.ToString()},
					{"rsatimestamp", ts},
					{"twofactorcode", string.IsNullOrEmpty(Token) ? authorization : SteamGuard.GenerateTwoFactorCode(Token)},
					{"username", User}
				};

				request.Content = new FormUrlEncodedContent(values);

				var x = httpPack.client.SendAsync(request).Result;

				if (!x.IsSuccessStatusCode)
					throw new HttpRequestException("x.IsSuccessStatusCode = false");

				responseString = x.Content.ReadAsStringAsync().Result;

				x?.Dispose();
				
				stuff = JsonConvert.DeserializeObject(responseString);

			}
			catch (Exception ex) { throw new HttpRequestException("SendLoginDataToSteam", ex); }

			if (stuff.success != "true")
				if (stuff.captcha_gid != "-1")
					throw new CaptchaNeededException(new Captcha() { gid = stuff.captcha_gid });
				else
					throw new MyLoginException("sendLoginDataToSteam", stuff ?? "");

			httpPack.handler.CookieContainer.Add(new Cookie("sessionid", RandomPart.GenerateSession(), "/", "steamcommunity.com"));

		}

		private void submitCookies() {

			try {

				var request = new HttpRequestMessage(HttpMethod.Post, "https://steamcommunity.com/");

				var x = httpPack.client.SendAsync(request).Result;

				if (!x.IsSuccessStatusCode)
					throw new HttpRequestException("x.IsSuccessStatusCode = false");

				x?.Dispose();

			}
			catch (Exception ex) { throw new HttpRequestException("SubmitCookies", ex); }

		}


		private string encryptPassword(string mod, string exp, string password) {

			try {

				var rsa = new RSACryptoServiceProvider();
				var rsaParameters = new RSAParameters {
					Exponent = HexToByte(exp),
					Modulus = HexToByte(mod)
				};

				rsa.ImportParameters(rsaParameters);

				byte[] bytePassword = Encoding.ASCII.GetBytes(password);
				byte[] encodedPassword = rsa.Encrypt(bytePassword, false);

				var result = Convert.ToBase64String(encodedPassword);

				if (string.IsNullOrEmpty(result))
					throw new NullReferenceException("result from encodedPassword is null or empty");

				return result;

			}
			catch (Exception ex) { throw new CryptographicException("encryptPassword", ex); }

		}

		private byte[] HexToByte(string hex) {
			int hexLen = hex.Length;
			if (hexLen % 2 == 1)
				throw new ArgumentException("The binary key cannot have an odd number of digits");

			byte[] ret = new byte[hexLen / 2];
			for (int i = 0; i < hexLen; i += 2)
				ret[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

			return ret;
		}

		private int GetHexVal(char hex) {
			int val = hex;
			return val - (val < 58 ? 48 : 55);
		}

	}
}