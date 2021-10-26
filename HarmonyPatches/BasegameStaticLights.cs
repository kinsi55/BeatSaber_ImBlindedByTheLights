using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using static PlayerSaveData;

namespace ImBlindedByTheLights.HarmonyPatches {
	[HarmonyPatch(typeof(BeatmapDataTransformHelper), nameof(BeatmapDataTransformHelper.CreateTransformedBeatmapData))]
	static class BasegameStaticLights {
		public static bool enabled { get; private set; } = false;
		static void Prefix(ref EnvironmentEffectsFilterPreset environmentEffectsFilterPreset) {
			if(Config.Instance.enablePlugin && Config.Instance.staticInHeadset && Config.Instance.staticOnDesktop)
				environmentEffectsFilterPreset = EnvironmentEffectsFilterPreset.NoEffects;

			enabled = environmentEffectsFilterPreset == EnvironmentEffectsFilterPreset.NoEffects;
		}
	}
}
