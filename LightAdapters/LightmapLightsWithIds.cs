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
		FieldAccessor<LightmapLightsWithIds, LightmapLightsWithIds.LightIntensitiesWithId[]>.Accessor FIELD_lightIntensityData =
			FieldAccessor<LightmapLightsWithIds, LightmapLightsWithIds.LightIntensitiesWithId[]>.GetAccessor("_lightIntensityData");

		Action<LightmapLightsWithIds> METHOD_HandleLightManagerDidChangeSomeColorsThisFrame = 
			MethodAccessor<LightWithIds, Action<LightWithIds>>.GetDelegate("HandleLightManagerDidChangeSomeColorsThisFrame");

		LightmapLightsWithIds.LightIntensitiesWithId[] _lightIntensityData;
		Color[] backupColors;

		public UnifiedLightmapLightsWithIds(LightmapLightsWithIds t) {
			this.t = t;
			_lightIntensityData = FIELD_lightIntensityData(ref t);
			backupColors = new Color[_lightIntensityData.Length];
		}

		public GameObject gameObject => t.gameObject;

		public void SetStatic() {
			for(var i = 0; i < _lightIntensityData.Length; i++) {
				backupColors[i] = _lightIntensityData[i].color;
				if(i != 3 && i != 0) {
					_lightIntensityData[i].ColorWasSet(Color.black);
				} else {
					_lightIntensityData[i].ColorWasSet(Config.Instance.staticColor);
				}
			}
			METHOD_HandleLightManagerDidChangeSomeColorsThisFrame(t);
		}

		public void Restore() {
			for(var i = 0; i < _lightIntensityData.Length; i++)
				_lightIntensityData[i].ColorWasSet(backupColors[i]);

			METHOD_HandleLightManagerDidChangeSomeColorsThisFrame(t);
		}
		public void SetActive(bool active) { }
		public bool CheckShouldBeActiveWhenStatic() => true;
	}
}
