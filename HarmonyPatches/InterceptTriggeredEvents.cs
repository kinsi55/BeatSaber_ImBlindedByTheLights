using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using IPA.Utilities;
using UnityEngine;

namespace ImBlindedByTheLights.HarmonyPatches {
	//[HarmonyPatch(typeof(BeatmapObjectCallbackController), nameof(BeatmapObjectCallbackController.SendBeatmapEventDidTriggerEvent))]
	//static class InterceptTriggeredEvents {
	//	static Dictionary<BeatmapEventType, BeatmapEventData> clearedEvents = new Dictionary<BeatmapEventType, BeatmapEventData>() {
	//		{ BeatmapEventType.Event0, new BeatmapEventData(0, BeatmapEventType.Event0, 1, 1) },
	//		{ BeatmapEventType.Event1, new BeatmapEventData(0, BeatmapEventType.Event1, 0, 0) },
	//		{ BeatmapEventType.Event2, new BeatmapEventData(0, BeatmapEventType.Event2, 0, 0) },
	//		{ BeatmapEventType.Event3, new BeatmapEventData(0, BeatmapEventType.Event3, 0, 0) },
	//		{ BeatmapEventType.Event4, new BeatmapEventData(0, BeatmapEventType.Event4, 1, 1) },
	//		{ BeatmapEventType.Event5, new BeatmapEventData(0, BeatmapEventType.Event5, 0, 0) },
	//		{ BeatmapEventType.Event6, new BeatmapEventData(0, BeatmapEventType.Event6, 0, 0) },
	//		{ BeatmapEventType.Event7, new BeatmapEventData(0, BeatmapEventType.Event7, 0, 0) },
	//		{ BeatmapEventType.Special0, new BeatmapEventData(0, BeatmapEventType.Special0, 0, 0) },
	//		{ BeatmapEventType.Special1, new BeatmapEventData(0, BeatmapEventType.Special1, 0, 0) },
	//		{ BeatmapEventType.Special2, new BeatmapEventData(0, BeatmapEventType.Special2, 0, 0) },
	//		{ BeatmapEventType.Special3, new BeatmapEventData(0, BeatmapEventType.Special3, 0, 0) }
	//	};
	//	static Dictionary<BeatmapEventType, BeatmapEventData> lastEvents = clearedEvents.ToDictionary(x => x.Key, x => x.Value);

	//	static BeatmapObjectCallbackController beatmapcallbackcontroller;

	//	static void Prefix(BeatmapObjectCallbackController __instance, ref BeatmapEventData beatmapEventData) {
	//		beatmapcallbackcontroller = __instance;

	//		var intType = (int)beatmapEventData.type;

	//		if(!(intType < (int)BeatmapEventType.Event8 || (intType >= (int)BeatmapEventType.Special0 && intType <= (int)BeatmapEventType.Special3)))
	//			return;

	//		if(lastEvents[beatmapEventData.type] == beatmapEventData)
	//			return;

	//		lastEvents[beatmapEventData.type] = beatmapEventData;

	//		Console.WriteLine("Triggered {0} {1} {2} {3}", beatmapEventData.time, beatmapEventData.type, beatmapEventData.value, beatmapEventData.floatValue);

	//		if(!wereLightsActive)
	//			beatmapEventData = clearedEvents[beatmapEventData.type];
	//	}

	//	static bool wereLightsActive = true;

	//	public static void EnableLights() {
	//		if(wereLightsActive)
	//			return;

	//		wereLightsActive = true;

	//		if(beatmapcallbackcontroller == null)
	//			return;

	//		for(var i = 0; i < lastEvents.Count; i++)
	//			beatmapcallbackcontroller.SendBeatmapEventDidTriggerEvent(lastEvents.Values.ElementAt(i));
	//	}

	//	public static void DisableLights() {
	//		if(!wereLightsActive)
	//			return;

	//		wereLightsActive = false;

	//		if(beatmapcallbackcontroller == null)
	//			return;

	//		for(var i = 0; i < clearedEvents.Count; i++)
	//			beatmapcallbackcontroller.SendBeatmapEventDidTriggerEvent(clearedEvents.Values.ElementAt(i));
	//	}
	//}

	//[HarmonyPatch]
	//static class a {
	//	static bool Prefix(BeatmapEventData beatmapEventData) => false;

	//	[HarmonyTargetMethods]
	//	static IEnumerable<MethodBase> TargetMethods() {
	//		yield return AccessTools.Method(typeof(LightSwitchEventEffect), nameof(LightSwitchEventEffect.HandleBeatmapObjectCallbackControllerBeatmapEventDidTrigger));
	//	}
	//}

	//[HarmonyPatch(typeof(MainEffectController), "OnPreRender")]
	//static class InterceptCameraRenders {
	//	static void Prefix(MainEffectController __instance) {
	//		if(__instance.GetComponent<Camera>().stereoTargetEye == StereoTargetEyeMask.None) {
	//			InterceptTriggeredEvents.DisableLights();
	//		} else {
	//			InterceptTriggeredEvents.EnableLights();
	//		}
	//	}
	//}



	[HarmonyPatch(typeof(LightSwitchEventEffect), nameof(LightSwitchEventEffect.SetColor))]
	public static class ToggleableLightSwitchEventEffect {
		public static LightWithIdManager[] managers = new LightWithIdManager[5];
		public static Color[] colors = new Color[5];

		static bool enableLights = true;

		private static readonly FieldAccessor<TubeBloomPrePassLight, ParametricBoxController>.Accessor TubeBloomPrePassLight_parametricBoxController = FieldAccessor<TubeBloomPrePassLight, ParametricBoxController>.GetAccessor("_parametricBoxController");

		public struct TubeLightsToYeet {
			public readonly TubeBloomPrePassLight a;
			public readonly TubeBloomPrePassLightWithId b;

			public bool yes;

			public TubeLightsToYeet(TubeBloomPrePassLightWithId b) {
				this.a = b.GetComponent<TubeBloomPrePassLight>();
				this.b = b;
				yes = true;
			}

			public void Enable(bool force = false) {
				if(b != null) b.enabled = true;
				if(a == null)
					return;

				if(force)
					a.color = Color.yellow;

				//a.enabled = true;
				a.Refresh();
			}

			public void Disable() {
				if(!yes)
					return;

				if(b != null) b.enabled = false;
				if(a == null)
					return;

				//a.enabled = false;
				a.Refresh();
			}
		}


		public static TubeLightsToYeet[] yeetedLights;
		public static TubeLightsToYeet[] forcedLights;

		public static void PrepareLightsArray() {
			yeetedLights =
				Resources.FindObjectsOfTypeAll<TubeBloomPrePassLightWithId>()
				.Where(x => {
					return x.name.Contains("Laser");

					var z = x.GetComponent<TubeBloomPrePassLight>();
					//var m = x.GetComponent<TubeBloomPrePassLightWithId>();

					//if(m != null && m.lightId == 5 && !m.gameObject.name.Contains("Laser"))
					//	return false;

					return x.transform.parent.name != "Environment" || TubeBloomPrePassLight_parametricBoxController(ref z) == null;

					//if(y == null || y.GetComponent<MeshRenderer>().material.name != "EnvLightOpaque")
					//	return false;
				})
				.Select(x => new TubeLightsToYeet(x))
				.ToArray();


			forcedLights =
				Resources.FindObjectsOfTypeAll<TubeBloomPrePassLightWithId>()
				.Where(x => {
					return !x.name.Contains("Laser");
				})
				.Select(x => new TubeLightsToYeet(x))
				.ToArray();
		}


		static void Prefix(int ____lightsID, LightWithIdManager ____lightManager, ref Color color) {
			managers[____lightsID - 1] = ____lightManager;
			colors[____lightsID - 1] = color;

			if(!enableLights)
				color = new Color(0, 0, 0, 0);
		}


		public static void EnableLights() {
			if(enableLights)
				return;

			enableLights = true;

			for(int i = 0; i < 5; i++) {
				if(managers[i] == null)
					continue;

				managers[i].SetColorForId(i + 1, colors[i]);
			}

			if(yeetedLights != null)
				foreach(var x in yeetedLights) x.Enable();
		}

		public static void DisableLights() {
			if(!enableLights)
				return;

			enableLights = false;

			for(int i = 0; i < 5; i++) {
				if(managers[i] == null)
					continue;

				if(i == 4 || i == 0) {
					managers[i].SetColorForId(i + 1, Color.yellow);
				} else {
					managers[i].SetColorForId(i + 1, new Color(0, 0, 0, 0));
				}
			}

			if(yeetedLights != null)
				foreach(var x in yeetedLights) x.Disable();

			if(forcedLights != null)
				foreach(var x in forcedLights) x.Enable(true);
		}


		[HarmonyPatch(typeof(LightWithIdManager), "LateUpdate")]
		static class FixLightsBeforeEndingFrame {
			static void Prefix() => EnableLights();
		}
	}


	[HarmonyPatch(typeof(MainEffectController), "OnPreRender")]
	static class InterceptCameraRenders {

		static void Prefix(MainEffectController __instance) {
			if(__instance.GetComponent<ImBlindedByTheLightsHandler>() == null) {
				__instance.gameObject.AddComponent<ImBlindedByTheLightsHandler>();
			}
		}
	}


	class ImBlindedByTheLightsHandler : MonoBehaviour {
		Camera cam;

		void Awake() {
			cam = GetComponent<Camera>();
		}

		void OnPreRender() {
			//Console.WriteLine("{0} OnPreRender", transform.parent?.name);

		}
		void OnPreCull() {
			if(cam.stereoTargetEye == StereoTargetEyeMask.None) {
				ToggleableLightSwitchEventEffect.EnableLights();
			} else {
				ToggleableLightSwitchEventEffect.DisableLights();
			}
		}

		//void OnRenderImage(RenderTexture a, RenderTexture b) {
		//	if(cam.stereoTargetEye != StereoTargetEyeMask.None) {
		//		ToggleableLightSwitchEventEffect.EnableLights();
		//	} else {
		//		ToggleableLightSwitchEventEffect.DisableLights();
		//	}
		//}
	}
}
