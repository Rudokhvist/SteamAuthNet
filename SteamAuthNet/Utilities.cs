// Copyright 2020 Andrii "Ryzhehvost" Kotlyar
// Contact: ryzhehvost@kotei.co.ua
// This derivative work is based on Utilities.cs from https://github.com/JustArchiNET/ArchiSteamFarm/tree/0ce04415
// -------------------------------------------------------------------------------------------------
// Copyright 2015-2020 Łukasz "JustArchi" Domeradzki
// Contact: JustArchi@JustArchi.net
// |
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// |
// http://www.apache.org/licenses/LICENSE-2.0
// |
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.XPath;
//TODO: Consider if I even need it
//using Humanizer;
//using Humanizer.Localisation;

namespace SteamAuthNet {
	public static class Utilities {
		private const byte TimeoutForLongRunningTasksInSeconds = 60;

		// Normally we wouldn't need to use this singleton, but we want to ensure decent randomness across entire program's lifetime
		private static readonly Random Random = new Random();


		public static string GetArgsAsText(string[] args, byte argsToSkip, string delimiter) {
			if ((args == null) || (args.Length <= argsToSkip) || string.IsNullOrEmpty(delimiter)) {
				return null;
			}

			return string.Join(delimiter, args.Skip(argsToSkip));
		}


		public static string GetArgsAsText(string text, byte argsToSkip) {
			if (string.IsNullOrEmpty(text)) {
				return null;
			}

			string[] args = text.Split((char[]) null, argsToSkip + 1, StringSplitOptions.RemoveEmptyEntries);

			return args[^1];
		}


		public static string GetAttributeValue(this INode node, string attributeName) {
			if ((node == null) || string.IsNullOrEmpty(attributeName)) {
				return null;
			}

			return node is IElement element ? element.GetAttribute(attributeName) : null;
		}


		public static uint GetUnixTime() => (uint) DateTimeOffset.UtcNow.ToUnixTimeSeconds();


		public static async void InBackground(Action action, bool longRunning = false) {
			if (action == null) {
				return;
			}

			TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;

			if (longRunning) {
				options |= TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;
			}

			await Task.Factory.StartNew(action, CancellationToken.None, options, TaskScheduler.Default).ConfigureAwait(false);
		}


		public static async void InBackground<T>(Func<T> function, bool longRunning = false) {
			if (function == null) {


				return;
			}

			TaskCreationOptions options = TaskCreationOptions.DenyChildAttach;

			if (longRunning) {
				options |= TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness;
			}

			await Task.Factory.StartNew(function, CancellationToken.None, options, TaskScheduler.Default).ConfigureAwait(false);
		}


		public static async Task<IList<T>> InParallel<T>(IEnumerable<Task<T>> tasks) {
			if (tasks == null) {


				return null;
			}

			IList<T> results;

			results = await Task.WhenAll(tasks).ConfigureAwait(false);

			return results;
		}


		public static async Task InParallel(IEnumerable<Task> tasks) {
			if (tasks == null) {


				return;
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);

		}


		public static bool IsClientErrorCode(this HttpStatusCode statusCode) => (statusCode >= HttpStatusCode.BadRequest) && (statusCode < HttpStatusCode.InternalServerError);


		public static bool IsValidCdKey(string key) {
			if (string.IsNullOrEmpty(key)) {


				return false;
			}

			return Regex.IsMatch(key, @"^[0-9A-Z]{4,7}-[0-9A-Z]{4,7}-[0-9A-Z]{4,7}(?:(?:-[0-9A-Z]{4,7})?(?:-[0-9A-Z]{4,7}))?$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
		}


		public static bool IsValidDigitsText(string text) {
			if (string.IsNullOrEmpty(text)) {


				return false;
			}

			return text.All(char.IsDigit);
		}


		public static bool IsValidHexadecimalText(string text) {
			if (string.IsNullOrEmpty(text)) {


				return false;
			}

			return (text.Length % 2 == 0) && text.All(Uri.IsHexDigit);
		}


		public static int RandomNext() {
			lock (Random) {
				return Random.Next();
			}
		}


		public static int RandomNext(int maxValue) {
			if (maxValue < 0) {
				throw new ArgumentOutOfRangeException(nameof(maxValue));
			}

			if (maxValue <= 1) {
				return maxValue;
			}

			lock (Random) {
				return Random.Next(maxValue);
			}
		}


		public static int RandomNext(int minValue, int maxValue) {
			if (minValue > maxValue) {
				throw new ArgumentOutOfRangeException(nameof(minValue) + " && " + nameof(maxValue));
			}

			if (minValue >= maxValue - 1) {
				return minValue;
			}

			lock (Random) {
				return Random.Next(minValue, maxValue);
			}
		}




		public static List<IElement> SelectElementNodes(this IElement element, string xpath) => element.SelectNodes(xpath).Cast<IElement>().ToList();




		public static List<IElement> SelectNodes(this IDocument document, string xpath) => document.Body.SelectNodes(xpath).Cast<IElement>().ToList();



		public static IElement SelectSingleElementNode(this IElement element, string xpath) => (IElement) element.SelectSingleNode(xpath);



		public static IElement SelectSingleNode(this IDocument document, string xpath) => (IElement) document.Body.SelectSingleNode(xpath);


		public static IEnumerable<T> ToEnumerable<T>(this T item) {
			yield return item;
		}


		//public static string ToHumanReadable(this TimeSpan timeSpan) => timeSpan.Humanize(3, maxUnit: TimeUnit.Year, minUnit: TimeUnit.Second);



		internal static void DeleteEmptyDirectoriesRecursively(string directory) {
			if (string.IsNullOrEmpty(directory)) {


				return;
			}

			if (!Directory.Exists(directory)) {
				return;
			}

			try {
				foreach (string subDirectory in Directory.EnumerateDirectories(directory)) {
					DeleteEmptyDirectoriesRecursively(subDirectory);
				}

				if (!Directory.EnumerateFileSystemEntries(directory).Any()) {
					Directory.Delete(directory);
				}
			} catch (Exception) {

			}
		}

		internal static string GetCookieValue(this CookieContainer cookieContainer, string url, string name) {
			if ((cookieContainer == null) || string.IsNullOrEmpty(url) || string.IsNullOrEmpty(name)) {
				return null;
			}

			Uri uri;

			try {
				uri = new Uri(url);
			} catch (UriFormatException) {
				return null;
			}

			CookieCollection cookies = cookieContainer.GetCookies(uri);

			return cookies.Count > 0 ? (from Cookie cookie in cookies where cookie.Name.Equals(name) select cookie.Value).FirstOrDefault() : null;
		}

		internal static bool RelativeDirectoryStartsWith(string directory, params string[] prefixes) {
			if (string.IsNullOrEmpty(directory) || (prefixes == null) || (prefixes.Length == 0)) {
				return false;
			}

			return (from prefix in prefixes where directory.Length > prefix.Length let pathSeparator = directory[prefix.Length] where (pathSeparator == Path.DirectorySeparatorChar) || (pathSeparator == Path.AltDirectorySeparatorChar) select prefix).Any(prefix => directory.StartsWith(prefix, StringComparison.Ordinal));
		}
	}
}
