using UnityEngine;

namespace ImBlindedByTheLights.LightAdapters {
#if DEBUG
	public
#endif
	interface ILightAdapter {
		public GameObject gameObject { get; }

		public void SetStatic();
		public void Restore();
		public void SetActive(bool active);
		public bool CheckShouldBeActiveWhenStatic();
	}
}
