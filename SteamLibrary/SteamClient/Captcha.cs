namespace SteamLibrary {
	public partial class SteamClient {
		public sealed class Captcha {
			public string gid = "-1";
			public string link => "https://steamcommunity.com/login/rendercaptcha/?gid=" + gid;
			public string text = "";
		}
	}
}