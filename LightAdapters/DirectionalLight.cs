using UnityEngine;

namespace ImBlindedByTheLights.LightAdapters {
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
		public bool CheckShouldBeActiveWhenStatic() => t.color.r < 0;
	}
}
