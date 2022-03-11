﻿using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ImBlindedByTheLights.LightAdapters;
using Object = UnityEngine.Object;

namespace ImBlindedByTheLights.HarmonyPatches {
	// While we are init-ing, force a very specific color to be set on stuff to make finding the lights more consistent
	[HarmonyPatch]
	static class ForceColorOnInit {
		public static readonly Color STATIC_COLOR = new Color(-1, 0, 0, 0.001f);
		public static bool enable = false;

		[HarmonyTargetMethods]
		static IEnumerable<MethodBase> TargetMethods() {
			yield return AccessTools.Method(typeof(LightSwitchEventEffect), nameof(LightSwitchEventEffect.GetNormalColor));
			yield return AccessTools.Method(typeof(LightSwitchEventEffect), nameof(LightSwitchEventEffect.GetHighlightColor));
		}

		static void Postfix(ref Color __result) {
			if(BasegameStaticLights.enabled || (!FilterBeatmapLightEvents.endedUpWithAnyLights && Config.Instance.staticWhenNoLights)) {
				if(Config.Instance.enablePlugin)
					__result = Config.Instance.staticColor;
				return;
			}

			if(enable && Config.Instance.enablePlugin)
				__result = STATIC_COLOR;
		}
	}

	[HarmonyPatch(typeof(TrackLaneRingsRotationEffect), nameof(TrackLaneRingsRotationEffect.Start))]
	static class ForceRingRotationSnap {
		// This is kinda dirty, "removes" the rotation that happens at the start of a level
		static void Prefix(ref float ____startupRotationStep, ref float ____startupRotationPropagationSpeed, ref float ____startupRotationFlexySpeed) {
			if((!Config.Instance.staticInHeadset && !Config.Instance.staticOnDesktop) || Config.Instance.keepRingSpinsInStatic || !Config.Instance.enablePlugin)
				return;

			____startupRotationStep *= 1f;
			____startupRotationPropagationSpeed *= 50f;
			____startupRotationFlexySpeed *= 4f;
		}
	}

#if DEBUG
	public
#endif
	static class LightSwitch {
		public static ILightAdapter[] staticLights;

		public static ILightAdapter[] yeetedLights;

		// Stuff that needs to not move
		static Transform[] fixedPositionThings;
		static Quaternion[] staticRotations;
		static Vector3[] staticPositions;

		static Quaternion[] staticRotationBackup;
		static Vector3[] staticPositionBackup;


		public static void Clear() {
			staticLights = null;
			yeetedLights = null;
			fixedPositionThings = null;
			lightsEnabled = true;
			ForceColorOnInit.enable = false;
		}

		public static IEnumerator Init() {
			/*
			 * First, to find out what actually is lit in the current Env when static lights is on, 
			 * act as tho we are on static lights!
			 */
			yield return null;
			var bcu = Object.FindObjectOfType<BeatmapCallbacksUpdater>();
			if(bcu == null)
				yield break;

			var controller = IPA.Utilities.ReflectionUtil.GetField<BeatmapCallbacksController, BeatmapCallbacksUpdater>(bcu, "_beatmapCallbacksController");

			if(controller == null)
				yield break;

			ForceColorOnInit.enable = true;
			yield return null;

			if(BasegameStaticLights.enabled) {
				ForceColorOnInit.enable = false;
				yield break;
			}

			// See BeatmapDataLoader::GetBeatmapDataFromBeatmapSaveData
			controller.TriggerBeatmapEvent(new BasicBeatmapEventData(0, BasicBeatmapEventType.Event0, 1, 1));
			controller.TriggerBeatmapEvent(new BasicBeatmapEventData(0, BasicBeatmapEventType.Event4, 1, 1));

			ForceColorOnInit.enable = false;

			if(Config.Instance.staticWhenNoLights && !FilterBeatmapLightEvents.endedUpWithAnyLights)
				yield break;

			// Now that this propagated to all the environment stuff, look for all the lights which are on

			var lights = Object.FindObjectsOfType<TubeBloomPrePassLight>().Select(x => (ILightAdapter)new UnifiedTubeBloomPrePassLight(x))
				.Concat(Object.FindObjectsOfType<LightmapLightsWithIds>().Select(x => new UnifiedLightmapLightsWithIds(x)))
				.Concat(Object.FindObjectsOfType<MaterialLightWithId>().Select(x => new UnifiedMaterialLightWithId(x)))
				.Concat(Object.FindObjectsOfType<DirectionalLight>().Select(x => new UnifiedDirectionalLight(x)))
				.Concat(Object.FindObjectsOfType<BloomPrePassBackgroundColorsGradient>().Select(x => new UnifiedBloomPrePassBackgroundColorsGradient(x)))
				.Concat(Object.FindObjectsOfType<SpriteLightWithId>().Select(x => new UnifiedSpriteLightWithId(x)))
				.Concat(Object.FindObjectsOfType<MaterialPropertyBlockColorSetter>().Select(x => new UnifiedMaterialPropertyBlockColorSetter(x)))

				//.Concat(Resources.FindObjectsOfTypeAll<MeshRenderer>().Where(x => x.material.HasProperty("_Color")).Select(x => new UnifiedMaterialColor(x)))
				.ToArray();

			staticLights = lights.Where(x => x.CheckShouldBeActiveWhenStatic()).ToArray();

			//foreach(var x in lights) {
			//	Console.WriteLine("{0} Start color {1}", x.gameObject.transform.GetPath(), x.color);
			//}


			yeetedLights = lights.Where(x => !x.CheckShouldBeActiveWhenStatic())
				.Concat(Object.FindObjectsOfType<ParticleSystemEventEffect>().Select(x => new UnifiedDynamicGameObject(x.gameObject)))
				.ToArray();

#if DEBUG
			Plugin.Log.Debug(string.Format("Found Lights! Static: {0} Dynamic: {1}", staticLights.Length, yeetedLights.Length));
#endif

			if(Config.Instance.keepRingSpinsInStatic)
				yield break;

			var _fixedPositionThings = Object.FindObjectsOfType<TrackLaneRing>().Select(x => x.transform)
				.Concat(Object.FindObjectsOfType<LightPairRotationEventEffect>().SelectMany(x => x.transform.Cast<Transform>()))
				.Where(x => x.gameObject.activeInHierarchy).Distinct().ToArray();

			staticRotationBackup = new Quaternion[_fixedPositionThings.Count()];
			staticPositionBackup = new Vector3[staticRotationBackup.Length];

			// We need to wait one more frame for these to be in the positions they should be
			yield return null;

			fixedPositionThings = _fixedPositionThings;
			staticRotations = _fixedPositionThings.Select(x => x.rotation).ToArray();
			staticPositions = _fixedPositionThings.Select(x => x.position).ToArray();
#if DEBUG
			Plugin.Log.Debug(string.Format("Found {0} fixedPositionThings!", fixedPositionThings.Length));
#endif
		}

		public static bool lightsEnabled { get; private set; } = true;

		public static void EnableLights() {
			if(lightsEnabled || staticLights == null || !Config.Instance.enablePlugin)
				return;

			lightsEnabled = true;

			for(var i = 0; i < yeetedLights.Length; i++)
				yeetedLights[i].SetActive(lightsEnabled);

			for(var i = 0; i < staticLights.Length; i++)
				staticLights[i].Restore();

			if(fixedPositionThings == null)
				return;

			for(var i = 0; i < fixedPositionThings.Length; i++) {
				fixedPositionThings[i].rotation = staticRotationBackup[i];
				fixedPositionThings[i].position = staticPositionBackup[i];
			}
		}

		public static void DisableLights() {
			if(!lightsEnabled || staticLights == null || !Config.Instance.enablePlugin)
				return;

			lightsEnabled = false;

			// A lot of the light thingy's have OnEnable/Disable methods - So SetActive instead of this is REALLY bad
			for(var i = 0; i < yeetedLights.Length; i++)
				yeetedLights[i].SetActive(lightsEnabled);

			for(var i = 0; i < staticLights.Length; i++)
				staticLights[i].SetStatic();

			if(fixedPositionThings == null)
				return;

			for(var i = 0; i < fixedPositionThings.Length; i++) {
				staticRotationBackup[i] = fixedPositionThings[i].rotation;
				staticPositionBackup[i] = fixedPositionThings[i].position;

				fixedPositionThings[i].rotation = staticRotations[i];
				fixedPositionThings[i].position = staticPositions[i];
			}
		}
	}

	// (Ab)using this (Which is on all normal cameras) to add our own behaviour
	[HarmonyPatch(typeof(BloomPrePass), "Awake")]
	static class InterceptCameraRenders {
		static void Postfix(BloomPrePass __instance) {
			var cam = __instance.GetComponent<Camera>();

			if(cam == null || __instance.gameObject.GetComponent<LightSwitchHandler>() != null)
				return;

			__instance.gameObject.AddComponent<LightSwitchHandler>();
		}
	}


	class LightSwitchHandler : MonoBehaviour {
		Camera cam;

		void Awake() {
			cam = GetComponent<Camera>();
		}

		int culls = 0;

		void LateUpdate() => culls = 0;

		void OnPreCull() {
			if(culls++ != 0)
				return;

			//System.Console.WriteLine("OnPreCull {0} {1} {2}", cam.transform?.parent?.name, cam.stereoTargetEye, cam.stereoActiveEye);

			if((cam.stereoActiveEye != Camera.MonoOrStereoscopicEye.Mono && Config.Instance.staticInHeadset) || (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Mono && Config.Instance.staticOnDesktop))
				LightSwitch.DisableLights();
		}

		void OnPostRender() {
			if(culls-- != 1)
				return;

			//System.Console.WriteLine("OnPostRender {0} {1} {2}", cam.transform?.parent?.name, cam.stereoTargetEye, cam.stereoActiveEye);

			LightSwitch.EnableLights();
		}
	}
}
