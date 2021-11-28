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
				var x = (List<BeatmapEventData>)__result.beatmapEventsData;

				for(var i = x.Count - 1; i > 0; i--) {
					/*
					 * Break affects the switchcase and removes the event
					 * Continue is for the For loop and leaves the event
					 */
					switch(x[i].type) {
						case BeatmapEventType.Event0:
							if(!Config.Instance.disableBackLasers) continue;
							break;
						case BeatmapEventType.Event1:
							if(!Config.Instance.disableBackLasers) continue;
							break;
						case BeatmapEventType.Event2:
						case BeatmapEventType.Event3:
						case BeatmapEventType.Event12:
						case BeatmapEventType.Event13:
							if(!Config.Instance.disableRotatingLasers) continue;
							break;
						case BeatmapEventType.Event4:
							if(!Config.Instance.disableCenterLights) continue;
							break;
						default:
							continue;
					}
					x.RemoveAt(i);
				}
			}

			endedUpWithAnyLights = false;

			if(Config.Instance.staticWhenNoLights) {
				foreach(var beatmapEventData in __result.beatmapEventsData) {
					var lType = (int)beatmapEventData.type;

					if(lType >= 0 && lType < 8 && (beatmapEventData.value > 0 || beatmapEventData.floatValue > 0)) {
						endedUpWithAnyLights = true;
						break;
					}
				}
			}
		}
	}
}
