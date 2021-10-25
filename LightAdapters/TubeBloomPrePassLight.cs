using UnityEngine;

namespace ImBlindedByTheLights.LightAdapters {
	class UnifiedTubeBloomPrePassLight : ILightAdapter {
		TubeBloomPrePassLight t;

		public UnifiedTubeBloomPrePassLight(TubeBloomPrePassLight t) {
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
		public bool CheckShouldBeActiveWhenStatic() => t.color.r < 0;
	}
}
