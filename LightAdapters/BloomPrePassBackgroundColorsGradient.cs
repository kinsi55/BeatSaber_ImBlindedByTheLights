using UnityEngine;

namespace ImBlindedByTheLights.LightAdapters {
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
		public void SetActive(bool active) { }

		public void Restore() => t.tintColor = backupColor;
		public bool CheckShouldBeActiveWhenStatic() => true;
	}
}
