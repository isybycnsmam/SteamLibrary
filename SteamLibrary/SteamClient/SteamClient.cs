using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;

namespace SteamLibrary {
	public sealed partial class SteamClient {
	
		public string User { get; private set; }
		public string Password { get; private set; }
		public string Token { get; private set; } = null;
		private HttpPack httpPack;

		public CookieCollection Cookies => GetAllCookies(httpPack.handler.CookieContainer);
		private CookieCollection GetAllCookies(CookieContainer container) {
			var allCookies = new CookieCollection();
			var domainTableField = container.GetType().GetRuntimeFields().FirstOrDefault(x => x.Name == "m_domainTable");
			var domains = (IDictionary)domainTableField.GetValue(container);

			foreach (var val in domains.Values) {
				var type = val.GetType().GetRuntimeFields().First(x => x.Name == "m_list");
				var values = (IDictionary)type.GetValue(val);
				foreach (CookieCollection cookies in values.Values) {
					allCookies.Add(cookies);
				}
			}
			return allCookies;
		}
		
	}
}