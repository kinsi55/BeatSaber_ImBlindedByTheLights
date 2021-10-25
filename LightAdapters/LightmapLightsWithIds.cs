using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace ImBlindedByTheLights.LightAdapters {
	class UnifiedLightmapLightsWithIds : ILightAdapter {
		LightmapLightsWithIds t;

		// Why is this private? Whats the getter for?!
		FieldInfo FIELD_lightIntensityData = AccessTools.Field(typeof(LightmapLightsWithIds), "_lightIntensityData");
		FieldInfo FIELD_color = AccessTools.Field(typeof(LightmapLightsWithIds.LightIntensitiesData), "_color");
		MethodInfo METHOD_lightIntensityData = AccessTools.Method(typeof(LightmapLightsWithIds), "HandleLightManagerDidChangeSomeColorsThisFrame");

		LightmapLightsWithIds.LightIntensitiesData[] _lightIntensityData;
		Color[] backupColors;

		public UnifiedLightmapLightsWithIds(LightmapLightsWithIds t) {
			this.t = t;
			_lightIntensityData = (LightmapLightsWithIds.LightIntensitiesData[])FIELD_lightIntensityData.GetValue(t);
			backupColors = new Color[_lightIntensityData.Length];
		}

		public GameObject gameObject => t.gameObject;

		public void SetStatic() {
			for(var i = 0; i < _lightIntensityData.Length; i++) {
				backupColors[i] = (Color)FIELD_color.GetValue(_lightIntensityData[i]);
				if(i != 3 && i != 0) {
					FIELD_color.SetValue(_lightIntensityData[i], Color.black);
				} else {
					FIELD_color.SetValue(_lightIntensityData[i], Config.Instance.staticColor);
				}
			}
			METHOD_lightIntensityData.Invoke(t, null);
		}

		public void Restore() {
			for(var i = 1; i < _lightIntensityData.Length; i++)
				FIELD_color.SetValue(_lightIntensityData[i], backupColors[i]);

			METHOD_lightIntensityData.Invoke(t, null);
		}
		public void SetActive(bool active) { }
		public bool CheckShouldBeActiveWhenStatic() => true;
	}
}
