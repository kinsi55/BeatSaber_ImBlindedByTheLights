using HarmonyLib;
using System.Linq;
using System;
using System.Collections.Generic;

namespace ImBlindedByTheLights.HarmonyPatches {
	[HarmonyPatch(typeof(BeatmapDataTransformHelper), nameof(BeatmapDataTransformHelper.CreateTransformedBeatmapData))]
	static class FilterBeatmapLightEvents {
		public static bool endedUpWithAnyLights { get; private set; } = false;

		static void Postfix(ref IReadonlyBeatmapData __result) {
			if(!Config.Instance.enablePlugin)
				return;

			if(Config.Instance.disableBackLasers || Config.Instance.disableCenterLights || Config.Instance.disableRingLights || Config.Instance.disableRotatingLasers) {
				__result.GetFilteredCopy(x => {
					if(!(x is BasicBeatmapEventData bbed))
						return x;

					switch(bbed.basicBeatmapEventType) {
						case BasicBeatmapEventType.Event0:
							if(Config.Instance.disableBackLasers) return null;
							break;
						case BasicBeatmapEventType.Event1:
							if(Config.Instance.disableBackLasers) return null;
							break;
						case BasicBeatmapEventType.Event2:
						case BasicBeatmapEventType.Event3:
						case BasicBeatmapEventType.Event12:
						case BasicBeatmapEventType.Event13:
							if(Config.Instance.disableRotatingLasers) return null;
							break;
						case BasicBeatmapEventType.Event4:
							if(Config.Instance.disableCenterLights) return null;
							break;
					}
					return x;
				});
			}

			endedUpWithAnyLights = false;

			if(Config.Instance.staticWhenNoLights) {
				foreach(var x in __result.allBeatmapDataItems) {
					if(!(x is BasicBeatmapEventData bbed))
						continue;

					var lType = (int)bbed.basicBeatmapEventType;

					if(lType >= 0 && lType < 8 && (bbed.value > 0 || bbed.floatValue > 0)) {
						endedUpWithAnyLights = true;
						break;
					}
				}
			}
		}
	}
}
