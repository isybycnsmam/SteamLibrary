using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace SteamLibrary {
	public sealed partial class SteamClient {

		/// <summary>
		/// Username used to log in
		/// </summary>
		public string User { get; private set; }
		/// <summary>
		/// Password used to log in
		/// </summary>
		public string Password { get; private set; }
		/// <summary>
		/// token (shared secret) is for generating two factor codes,
		/// See <a href="https://github.com/SteamTimeIdler/stidler/wiki/Getting-your-%27shared_secret%27-code-for-use-with-Auto-Restarter-on-Mobile-Authentication">How to get</a>
		/// 
		/// </summary>
		public string Token { get; private set; } = null;

		private HttpClientHandler handler;
		private HttpClient client;

		/// <summary>
		/// Cookies from HttpClient
		/// </summary>
		public CookieCollection Cookies {
			get {
				var allCookies = new CookieCollection();
				var domainTableField = handler.CookieContainer.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == "m_domainTable");
				var domains = (IDictionary)domainTableField.GetValue(handler.CookieContainer);

				foreach (var val in domains.Values) {
					var type = val.GetType().GetRuntimeFields().First(x => x.Name == "m_list");
					var values = (IDictionary)type.GetValue(val);
					foreach (CookieCollection cookies in values.Values)
						allCookies.Add(cookies);
				}
				return allCookies;
			}
		}
		
		public SteamClient() => resetHttpClient();
		private void resetHttpClient() {
			var cookies = new CookieCollection();
			if (handler != null)
				cookies = Cookies;
			handler = new HttpClientHandler();
			client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
			foreach (Cookie cookie in cookies)
				if (cookie.Domain == "steamcommunity.com")
					handler.CookieContainer.Add(cookie);
		}

		public HttpClient GetHttpClient() => client;
		public HttpClientHandler GetHttpClientHandler() => handler;

	}
}