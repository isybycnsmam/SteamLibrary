namespace SteamLibrary {
	public partial class SteamClient {
		/// <summary>
		/// Class that contains all info about captcha that you must fill and pass again into Login function
		/// </summary>
		/// <seealso cref="Login"/>
		public sealed class Captcha {
			public string gid = "-1";
			public string link => "https://steamcommunity.com/login/rendercaptcha/?gid=" + gid;
			public string text = "";
		}
	}
}