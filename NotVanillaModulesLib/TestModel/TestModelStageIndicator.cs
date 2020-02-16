using UnityEngine;

namespace NotVanillaModulesLib.TestModel {
	public class TestModelStageIndicator : MonoBehaviour {
		public GameObject[] LightsOff;
		public GameObject[] LightsOn;

		private int stage;
		public int Stage {
			get => this.stage;
			set {
				this.stage = value;
				for (int i = 0; i < this.LightsOff.Length; ++i) {
					if (i < value) {
						this.LightsOn[i].SetActive(true);
						this.LightsOff[i].SetActive(false);
					} else {
						this.LightsOn[i].SetActive(false);
						this.LightsOff[i].SetActive(true);
					}
				}
			}
		}
	}
}
