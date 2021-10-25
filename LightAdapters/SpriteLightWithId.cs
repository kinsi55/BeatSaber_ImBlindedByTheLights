using UnityEngine;

namespace ImBlindedByTheLights.LightAdapters {
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
		public bool CheckShouldBeActiveWhenStatic() => t.color.r < 0;
	}
}
