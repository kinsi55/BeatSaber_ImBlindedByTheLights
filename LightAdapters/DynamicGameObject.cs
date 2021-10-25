using UnityEngine;

namespace ImBlindedByTheLights.LightAdapters {
	class UnifiedDynamicGameObject : ILightAdapter {
		public GameObject gameObject { get; private set; }

		public UnifiedDynamicGameObject(GameObject t) {
			gameObject = t;
		}

		public void SetStatic() => SetActive(false);

		public void Restore() => SetActive(true);
		public void SetActive(bool active) => gameObject.SetActive(active);
		public bool CheckShouldBeActiveWhenStatic() => false;
	}
}
