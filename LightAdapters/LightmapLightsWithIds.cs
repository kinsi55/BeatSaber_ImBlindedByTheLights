using HarmonyLib;
using System.Reflection;
using UnityEngine;
using IPA.Utilities;
using System;
using System.Linq;

namespace ImBlindedByTheLights.LightAdapters {
	class UnifiedLightmapLightsWithIds : ILightAdapter {
		LightmapLightsWithIds t;

		// Why is this private? Whats the getter for?!
		FieldAccessor<LightmapLightsWithIds, LightmapLightsWithIds.LightIntensitiesData[]>.Accessor FIELD_lightIntensityData =
			FieldAccessor<LightmapLightsWithIds, LightmapLightsWithIds.LightIntensitiesData[]>.GetAccessor("_lightIntensityData");

		FieldAccessor<LightWithIds.LightData, Color>.Accessor FIELD_color =
			FieldAccessor<LightWithIds.LightData, Color>.GetAccessor("_color");

		Action<LightmapLightsWithIds> METHOD_lightIntensityData = MethodAccessor<LightmapLightsWithIds, Action<LightmapLightsWithIds>>.GetDelegate("HandleLightManagerDidChangeSomeColorsThisFrame");

		LightWithIds.LightData[] _lightIntensityData;
		Color[] backupColors;

		public UnifiedLightmapLightsWithIds(LightmapLightsWithIds t) {
			this.t = t;
			_lightIntensityData = FIELD_lightIntensityData(ref t).Cast<LightWithIds.LightData>().ToArray();
			backupColors = new Color[_lightIntensityData.Length];
		}

		public GameObject gameObject => t.gameObject;

		public void SetStatic() {
			for(var i = 0; i < _lightIntensityData.Length; i++) {
				backupColors[i] = FIELD_color(ref _lightIntensityData[i]);
				if(i != 3 && i != 0) {
					FIELD_color(ref _lightIntensityData[i]) = Color.black;
				} else {
					FIELD_color(ref _lightIntensityData[i]) = Config.Instance.staticColor;
				}
			}
			METHOD_lightIntensityData(t);
		}

		public void Restore() {
			for(var i = 0; i < _lightIntensityData.Length; i++)
				FIELD_color(ref _lightIntensityData[i]) = backupColors[i];

			METHOD_lightIntensityData(t);
		}
		public void SetActive(bool active) { }
		public bool CheckShouldBeActiveWhenStatic() => true;
	}
}
