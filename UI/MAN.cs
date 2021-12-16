using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace ImBlindedByTheLights.UI {
	class MAN {
		Config config = Config.Instance;

		private readonly string version = $"Version {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} by Kinsi55\nCommissioned by NoneTaken";

		[UIComponent("sponsorsText")] CurvedTextMeshPro sponsorsText = null;
		void OpenSponsorsLink() => Process.Start("https://github.com/sponsors/kinsi55");
		void OpenSponsorsModal() {
			sponsorsText.text = "Loading...";
			Task.Run(() => {
				string desc = "Failed to load";
				try {
					desc = (new WebClient()).DownloadString("http://kinsi.me/sponsors/bsout.php");
				} catch { }

				_ = IPA.Utilities.Async.UnityMainThreadTaskScheduler.Factory.StartNew(() => {
					sponsorsText.text = desc;
					// There is almost certainly a better way to update / correctly set the scrollbar size...
					sponsorsText.gameObject.SetActive(false);
					sponsorsText.gameObject.SetActive(true);
				});
			}).ConfigureAwait(false);
		}
	}
}
