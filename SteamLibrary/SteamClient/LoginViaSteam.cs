using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using SteamLibrary.Exceptions;

namespace SteamLibrary {
	public partial class SteamClient {

		/// <summary>
		/// This method log you into any steam based site without cloudflare
		/// </summary>
		/// <exception cref="HttpRequestException">Error in sending requests, contains inner exception</exception>
		/// <exception cref="InvalidRequestCallbackException">After sending request the calback contains unexpected value, contains </exception>
		/// <param name="loginPage">Link to the login page</param>
		/// <param name="customCookies">Cookies that you can append to client</param>
		/// <param name="resetCookies">True to clear all cookies without steam cookies</param>
		/// <returns>Page session cookies</returns>
		public CookieCollection LoginViaSteam(string loginPage, CookieCollection customCookies = null, bool resetCookies = false) {

			if (resetCookies)
				resetHttpClient();

			handler.CookieContainer.Add(customCookies ?? new CookieCollection());

			var request = new HttpRequestMessage(HttpMethod.Get, loginPage);
			var response = client.SendAsync(request).Result.EnsureSuccessStatusCode();
			if (response.RequestMessage.RequestUri.Host != "steamcommunity.com")
				throw new InvalidRequestCallbackException("result uri is not steam page, you may be already logged", response.RequestMessage.RequestUri);


			var responseString = response.Content.ReadAsStringAsync().Result;
			var action = extractString(responseString, "name=\"action\" value=\"", "\" />");
			var openidMode = extractString(responseString, "openid.mode\" value=\"", "\" />");
			var openidParams = extractString(responseString, "openidparams\" value=\"", "\" />");
			var nonce = extractString(responseString, "nonce\" value=\"", "\" />");

			if (string.IsNullOrEmpty(action) || string.IsNullOrEmpty(openidMode) || string.IsNullOrEmpty(openidParams) || string.IsNullOrEmpty(nonce))
				throw new Exception("the responce does not contain necessary post data, you may be logged out");


			MultipartFormDataContent form = new MultipartFormDataContent();
			form.Add(new StringContent(action), "action");
			form.Add(new StringContent(openidMode), "openid.mode");
			form.Add(new StringContent(openidParams), "openidparams");
			form.Add(new StringContent(nonce), "nonce");

			request = new HttpRequestMessage(HttpMethod.Post, "https://steamcommunity.com/openid/login") { Content = form };
			response = client.SendAsync(request).Result.EnsureSuccessStatusCode();
			if (response.RequestMessage.RequestUri.Host == "steamcommunity.com")
				throw new InvalidRequestCallbackException("resulturi is still steamcomunnity.com", response.RequestMessage.RequestUri);

			var resultCookies = new CookieCollection();
			foreach (Cookie cookie in Cookies)
				if (cookie.Domain != "steamcommunity.com")
					resultCookies.Add(cookie);

			return resultCookies;

		}

		/// <summary>
		/// This method refers to LoginViaSteam but first prepares cookies and is based on /login-steam path
		/// </summary>
		/// <param name="pageLoginLink">this function supports only /login-steam path based sites</param>
		/// <param name="customCookies">Cookies that you can append to client</param>
		/// <param name="resetCookies">True to clear all cookies without steam cookies\</param>
		/// <returns>Page session cookies</returns>
		public CookieCollection AutomaticLoginViaSteam(string pageLoginLink, CookieCollection customCookies = null, bool resetCookies = false) {

			if (resetCookies)
				resetHttpClient();

			handler.CookieContainer.Add(customCookies ?? new CookieCollection());

			var uri = new Uri(pageLoginLink);
			var origin = $"{uri.Scheme}://{uri.Host}";

			client.GetAsync(origin).Result.EnsureSuccessStatusCode();

			//cloudflare? ===============================================


			if (uri.AbsolutePath == "/login-steam") {

				//sending request to get steam page
				var request = new HttpRequestMessage(HttpMethod.Post, pageLoginLink);
				request.Headers.Add("Origin", origin);
				request.Headers.Host = uri.Host;
				var result = client.SendAsync(request).Result.EnsureSuccessStatusCode();

				//get path hopefully to steam login page
				var location = result.RequestMessage.RequestUri.ToString();

				Console.WriteLine(location);

				if (new Uri(location).Host != "steamcommunity.com")
					throw new InvalidRequestCallbackException("returned adress isn't steam page", new Uri(location));

				return LoginViaSteam(location);
			}
			else
				throw new NotImplementedException("Automatic method can not recognise this method");

		}

		private string extractString(string content, string startString, string endString) {
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

	}
}