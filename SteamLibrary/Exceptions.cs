using System;

namespace SteamLibrary.Exceptions {
	public sealed class MyLoginException						: Exception { public MyLoginException(string msg, string json) : base(msg) { Json = json; } public string Json { get; private set; } }
	public sealed class CaptchaNeededException					: Exception { public CaptchaNeededException(SteamClient.Captcha captcha) { this.captcha = captcha; } public SteamClient.Captcha captcha; }
	public sealed class InvalidRequestCallbackException         : Exception { public InvalidRequestCallbackException(string msg, Uri resulturi) : base(msg) { ResultUri = resulturi; } public Uri ResultUri { get; private set; } }
}