using ImBlindedByTheLights.HarmonyPatches;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using ImBlindedByTheLights.UI;


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

		public void Bench(Action a) {
			var sw = new System.Diagnostics.Stopwatch();
			sw.Start();
			for(var i = 0; i < 500; i++) {
				a();
			}
			sw.Stop();

			Console.WriteLine("Took {0}", sw.ElapsedMilliseconds);
		}
	}

	//public static class lol {
	//	public static string GetPath(this Transform current) {
	//		if(current.parent == null)
	//			return "/" + current.name;
	//		return current.parent.GetPath() + "/" + current.name;
	//	}
	//}
}
