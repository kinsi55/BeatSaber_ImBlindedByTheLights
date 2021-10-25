using UnityEngine;

namespace ImBlindedByTheLights.LightAdapters {
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
		public bool CheckShouldBeActiveWhenStatic() => t.color.r < 0;
	}
}
