using HarmonyLib;

namespace ImBlindedByTheLights.HarmonyPatches {
	[HarmonyPatch(typeof(BeatmapDataTransformHelper), nameof(BeatmapDataTransformHelper.CreateTransformedBeatmapData))]
	static class FilterBeatmapLightEvents {
		static void Postfix(ref IReadonlyBeatmapData __result) {
			if(!Config.Instance.enablePlugin)
				return;

			if(!Config.Instance.disableBackLasers && !Config.Instance.disableCenterLights && !Config.Instance.disableRingLights && !Config.Instance.disableRotatingLasers)
				return;

			BeatmapData copyWithoutEvents = __result.GetCopyWithoutEvents();

			foreach(BeatmapEventData beatmapEventData in __result.beatmapEventsData) {
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
	}
}
