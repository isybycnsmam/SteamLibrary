using System;

namespace SteamLibrary {
	internal static class RandomPart {

		private static Random rnd = new Random();

		public static string GenerateSession() {

			var result = "";

			for (int i = 0; i < 24; i++)
				result += rnd.Next(0, 2) == 0 ? (char)rnd.Next('0', '9' + 1) : (char)rnd.Next('a', 'f' + 1);

			return result;

		}

	}
}