using HarmonyLib;
using ImBlindedByTheLights.HarmonyPatches;
using ImBlindedByTheLights.UI;
using IPA;
using IPA.Config.Stores;
using System.Reflection;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;


// BeatmapDataLoader::GetBeatmapDataFromBeatmapSaveData

/*
	if (beatmapData.beatmapEventsData.Count == 0) {
		beatmapData.AddBeatmapEventData(new BeatmapEventData(0f, BeatmapEventType.Event0, 1, 1f));
		beatmapData.AddBeatmapEventData(new BeatmapEventData(0f, BeatmapEventType.Event4, 1, 1f));
	}
*/

namespace ImBlindedByTheLights {
	[Plugin(RuntimeOptions.SingleStartInit)]
	public class Plugin {
		internal static Plugin Instance { get; private set; }
		internal static IPALogger Log { get; private set; }
		internal static Harmony harmony { get; private set; }

		[Init]
		public void Init(IPALogger logger, IPA.Config.Config conf) {
			Instance = this;
			Log = logger;
			Config.Instance = conf.Generated<Config>();
		}

		[OnStart]
		public void OnApplicationStart() {
			harmony = new Harmony("Kinsi55.BeatSaber.ImBlindedByTheLights");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

			BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup.instance.AddTab("BlindedByTheLights", "ImBlindedByTheLights.UI.GamePlaySetupTab.bsml", new MAN());
		}

		private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1) {
			if(Config.Instance.enablePlugin && arg1.name == "GameCore" && (Config.Instance.staticInHeadset || Config.Instance.staticOnDesktop)) {
				SharedCoroutineStarter.instance.StartCoroutine(LightSwitch.Init());
			} else {
				LightSwitch.Clear();
			}
		}

		[OnExit]
		public void OnApplicationQuit() {
			harmony.UnpatchAll(harmony.Id);
		}
	}
}
