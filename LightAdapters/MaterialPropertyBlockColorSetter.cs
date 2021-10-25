using UnityEngine;

namespace ImBlindedByTheLights.LightAdapters {
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
		public bool CheckShouldBeActiveWhenStatic() => t.color.a > 0 && t.color.a < 0.003f;
	}
}
