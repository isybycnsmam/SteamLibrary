using System;
using System.Net;
using System.Net.Http;
using SteamLibrary.Exceptions;

namespace SteamLibrary {
	public partial class SteamClient {
	
		public class HttpPack {

			public HttpClientHandler handler;
			public HttpClient client;

			public HttpPack() {
				handler = new HttpClientHandler();
				client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
				client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
			}

		}

		private string ExtractString(string content, string startString, string endString) {
			int Start = 0, End = 0;
			if (content.Contains(startString) && content.Contains(endString)) {
				Start = content.IndexOf(startString, 0) + startString.Length;
				End = content.IndexOf(endString, Start);

				string Hresult = content.Substring(Start, End - Start);

				return Hresult;
			}
			else
				return string.Empty;
		}

		/// <summary>
		/// This method login you to any steam based site without cloudflare
		/// </summary>
		/// <exception cref="HttpRequestException">Error in sending requests, contains inner exception</exception>
		/// <exception cref="InvalidRequestCallbackException">After sending request the calback contains unexpected value, contains </exception>
		/// <param name="loginPage">Link to the login page</param>
		/// <param name="customCookies">Cookies that you can append to client</param>
		/// <param name="httpPack">Custom http client and handler, can be used to implement proxy and more</param>
		/// <returns>Page session cookies</returns>
		public CookieCollection LoginViaSteam(string loginPage, CookieCollection customCookies = null) {

			var httpPack = new HttpPack();

			httpPack.handler.CookieContainer.Add(customCookies ?? new CookieCollection());

			return doRequest(loginPage, ref httpPack);
			
		}

		/// <summary>
		/// This method login you to any steam based site without cloudflare
		/// Supports custom HttpPack to implement proxy etc.
		/// </summary>
		/// <exception cref="HttpRequestException">Error in sending requests, contains inner exception</exception>
		/// <exception cref="InvalidRequestCallbackException">After sending request the calback contains unexpected value, contains </exception>
		/// <param name="loginPage">Link to the login page</param>
		/// <param name="httpPack">Custom http client and handler</param>
		/// <returns>Page session cookies</returns>
		public CookieCollection LoginViaSteam(string loginPage, ref HttpPack httpPack, CookieCollection customCookies = null) {

			httpPack.handler.CookieContainer.Add(customCookies ?? new CookieCollection());

			return doRequest(loginPage, ref httpPack);

		}


		public CookieCollection doRequest(string loginPage, ref HttpPack httpPack){

			httpPack.handler.CookieContainer.Add(Cookies);

			var x = new HttpResponseMessage();

			var y = new HttpResponseMessage();


			try {

				var request = new HttpRequestMessage(HttpMethod.Get, loginPage);

				x = httpPack.client.SendAsync(request).Result;

				if (!x.IsSuccessStatusCode)
					throw new HttpRequestException("x.IsSuccessStatusCode = false");

			}
			catch (Exception ex) { throw new HttpRequestException("LoginViaSteam", ex); }

			if (x.RequestMessage.RequestUri.Host != "steamcommunity.com")
				throw new InvalidRequestCallbackException("result uri is not steam page, you may be already logged", x.RequestMessage.RequestUri);

			try {

				var responseString = x.Content.ReadAsStringAsync().Result;

				var action = ExtractString(responseString, "name=\"action\" value=\"", "\" />");
				var openidMode = ExtractString(responseString, "openid.mode\" value=\"", "\" />");
				var openidParams = ExtractString(responseString, "openidparams\" value=\"", "\" />");
				var nonce = ExtractString(responseString, "nonce\" value=\"", "\" />");

				if (string.IsNullOrEmpty(action) || string.IsNullOrEmpty(openidMode) || string.IsNullOrEmpty(openidParams) || string.IsNullOrEmpty(nonce))
					throw new Exception("the responce does not contain necessary post data, you may be logged out");


				MultipartFormDataContent form = new MultipartFormDataContent();
				form.Add(new StringContent(action), "action");
				form.Add(new StringContent(openidMode), "openid.mode");
				form.Add(new StringContent(openidParams), "openidparams");
				form.Add(new StringContent(nonce), "nonce");

				var request = new HttpRequestMessage(HttpMethod.Post, "https://steamcommunity.com/openid/login") { Content = form };

				y = httpPack.client.SendAsync(request).Result;

				if (!y.IsSuccessStatusCode)
					throw new HttpRequestException("y.IsSuccessStatusCode = false");

			}
			catch (Exception ex) { throw new HttpRequestException("LoginViaSteam", ex); }

			if (y.RequestMessage.RequestUri.Host == "steamcommunity.com")
				throw new InvalidRequestCallbackException("resulturi is still steamcomunnity.com", y.RequestMessage.RequestUri);

			try {

				var retCook = new CookieCollection();

				foreach (Cookie cookie in GetAllCookies(httpPack.handler.CookieContainer))
					if (cookie.Domain != "steamcommunity.com")
						retCook.Add(cookie);

				return retCook;

			}
			catch (Exception ex) { throw new HttpRequestException("LoginViaSteam", ex); }

		}

	}
}