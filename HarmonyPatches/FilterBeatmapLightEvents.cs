﻿using HarmonyLib;
using System.Linq;

namespace ImBlindedByTheLights.HarmonyPatches {
	[HarmonyPatch(typeof(BeatmapDataTransformHelper), nameof(BeatmapDataTransformHelper.CreateTransformedBeatmapData))]
	static class FilterBeatmapLightEvents {
		public static bool endedUpWithAnyLights { get; private set; } = false;

		static void Postfix(ref IReadonlyBeatmapData __result) {
			if(!Config.Instance.enablePlugin)
				return;

			if(Config.Instance.disableBackLasers || Config.Instance.disableCenterLights || Config.Instance.disableRingLights || Config.Instance.disableRotatingLasers) {
				BeatmapData copyWithoutEvents = __result.GetCopyWithoutEvents();

				foreach(var beatmapEventData in __result.beatmapEventsData.OrderBy(x => x.time)) {
					switch(beatmapEventData.type) {
						case BeatmapEventType.Event0:
							if(Config.Instance.disableBackLasers) continue;
							break;
						case BeatmapEventType.Event1:
							if(Config.Instance.disableBackLasers) continue;
							break;
						case BeatmapEventType.Event2:
						case BeatmapEventType.Event3:
						case BeatmapEventType.Event12:
						case BeatmapEventType.Event13:
							if(Config.Instance.disableRotatingLasers) continue;
							break;
						case BeatmapEventType.Event4:
							if(Config.Instance.disableCenterLights) continue;
							break;
					}
					copyWithoutEvents.AddBeatmapEventData(beatmapEventData);
				}

				__result = copyWithoutEvents;
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
