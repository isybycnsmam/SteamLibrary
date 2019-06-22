using System;

namespace SteamLibrary.Exceptions {
	public sealed class CaptchaNeededException : Exception {
		public CaptchaNeededException(SteamClient.Captcha captcha) => this.captcha = captcha;
		public SteamClient.Captcha captcha;
	}
	public sealed class InvalidRequestCallbackException : Exception {
		public InvalidRequestCallbackException(string msg, Uri resulturi) : base(msg) => ResultUri = resulturi;
		public Uri ResultUri { get; private set; }
	}
}