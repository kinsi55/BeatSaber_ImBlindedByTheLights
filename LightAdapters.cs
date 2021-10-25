using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ImBlindedByTheLights {
	public interface ILightAdapter {
		public GameObject gameObject { get; }

		public void SetStatic();
		public void Restore();
		public void SetActive(bool active);
		public bool CheckIsStatic();
	}

	class UnifiedTubelight : ILightAdapter {
		TubeBloomPrePassLight t;

		public UnifiedTubelight(TubeBloomPrePassLight t) {
			this.t = t;
		}

		public Color color {
			set {
				t.color = value;
				t.Refresh();
			}
		}
		public GameObject gameObject => t.gameObject;

		Color backupColor;

		public void SetStatic() {
			backupColor = t.color;
			color = Config.Instance.staticColor;
		}

		public void SetActive(bool active) {
			t.transform.position += new Vector3(0, active ? 2000 : -2000);
		}

		public void Restore() => color = backupColor;
		public bool CheckIsStatic() => t.color.r < 0;
	}

	class UnifiedDirectionalLight : ILightAdapter {
		DirectionalLight t;
		public UnifiedDirectionalLight(DirectionalLight t) {
			this.t = t;
		}

		public GameObject gameObject => t.gameObject;

		Color backupColor;

		public void SetStatic() {
			backupColor = t.color;
			t.color = Config.Instance.staticColor;
		}
		public void SetActive(bool active) {
			if(active) {
				t.color = backupColor;
			} else {
				backupColor = t.color;
				t.color = Color.black;
			}
		}

		public void Restore() => t.color = backupColor;
		public bool CheckIsStatic() => t.color.r < 0;
	}

	class UnifiedMaterialLightWithId : ILightAdapter {
		MaterialLightWithId t;
		
		public UnifiedMaterialLightWithId(MaterialLightWithId t) {
			this.t = t;
		}


		public GameObject gameObject => t.gameObject;

		Color backupColor;

		public void SetStatic() {
			backupColor = t.color;
			t.ColorWasSet(Config.Instance.staticColor);
		}
		public void SetActive(bool active) {
			t.transform.position += new Vector3(0, active ? 2000 : -2000);
		}

		public void Restore() => t.ColorWasSet(backupColor);
		public bool CheckIsStatic() => t.color.r < 0;
	}

	class UnifiedMaterialColor : ILightAdapter {
		Material t;
		public GameObject gameObject { get; private set; }

		public UnifiedMaterialColor(MeshRenderer r) {
			t = r.material;
			gameObject = r.gameObject;
			staticColor = t.color;
		}


		Color backupColor;
		Color staticColor;

		public void SetStatic() {
			backupColor = t.color;
			t.color = staticColor;
		}
		public void SetActive(bool active) {}

		public void Restore() => t.color = backupColor;
		public bool CheckIsStatic() => t.color.r > 0;
	}

	class UnifiedBloomPrePassBackgroundColorsGradient : ILightAdapter {
		BloomPrePassBackgroundColorsGradient t;

		public UnifiedBloomPrePassBackgroundColorsGradient(BloomPrePassBackgroundColorsGradient t) {
			this.t = t;
			staticColor = t.tintColor;
		}

		public GameObject gameObject => t.gameObject;


		Color backupColor;
		Color staticColor;

		public void SetStatic() {
			backupColor = t.tintColor;
			t.tintColor = staticColor;
		}
		public void SetActive(bool active) {}

		public void Restore() => t.tintColor = backupColor;
		public bool CheckIsStatic() => true;
	}

	class UnifiedMaterialPropertyBlockColorSetter : ILightAdapter {
		MaterialPropertyBlockColorSetter t;

		public UnifiedMaterialPropertyBlockColorSetter(MaterialPropertyBlockColorSetter t) {
			this.t = t;
		}

		public GameObject gameObject => t.gameObject;


		Color backupColor;

		public void SetStatic() {
			backupColor = t.color;
			t.SetColor(Config.Instance.staticColor);
		}
		public void SetActive(bool active) { }

		public void Restore() => t.SetColor(backupColor);
		public bool CheckIsStatic() => t.color.a > 0 && t.color.a < 0.003f;
	}

	class UnifiedSpriteLightWithId : ILightAdapter {
		SpriteLightWithId t;

		public UnifiedSpriteLightWithId(SpriteLightWithId t) {
			this.t = t;
		}

		public Color color {
			get => t.color;
			set => t.ColorWasSet(value);
		}
		public GameObject gameObject => t.gameObject;


		Color backupColor;

		public void SetStatic() {
			backupColor = t.color;
			t.ColorWasSet(Config.Instance.staticColor);
		}
		public void SetActive(bool active) {
			t.transform.position += new Vector3(0, active ? 2000 : -2000);
		}

		public void Restore() => t.ColorWasSet(backupColor);
		public bool CheckIsStatic() => t.color.r < 0;
	}

	public class UnifiedLightmapLightsWithIds : ILightAdapter {
		LightmapLightsWithIds t;

		// Why is this private? Whats the getter for?!
		FieldInfo FIELD_lightIntensityData = AccessTools.Field(typeof(LightmapLightsWithIds), "_lightIntensityData");
		FieldInfo FIELD_color = AccessTools.Field(typeof(LightmapLightsWithIds.LightIntensitiesData), "_color");
		MethodInfo METHOD_lightIntensityData = AccessTools.Method(typeof(LightmapLightsWithIds), "HandleLightManagerDidChangeSomeColorsThisFrame");

		LightmapLightsWithIds.LightIntensitiesData[] _lightIntensityData;
		Color[] backupColors;

		public static bool enable = true;

		public UnifiedLightmapLightsWithIds(LightmapLightsWithIds t) {
			this.t = t;
			_lightIntensityData = (LightmapLightsWithIds.LightIntensitiesData[])FIELD_lightIntensityData.GetValue(t);
			backupColors = new Color[_lightIntensityData.Length];
		}

		public GameObject gameObject => t.gameObject;

		public void SetStatic() {
			if(!enable)
				return;

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
			for(var i = 1; i < _lightIntensityData.Length; i++) {
				FIELD_color.SetValue(_lightIntensityData[i], backupColors[i]);
			}
			METHOD_lightIntensityData.Invoke(t, null);
		}
		public void SetActive(bool active) {}
		public bool CheckIsStatic() => true;
	}
}
