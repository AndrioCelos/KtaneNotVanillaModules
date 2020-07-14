using UnityEngine;

namespace NotVanillaModulesLib.TestModel {
	public class TestModelLED : MonoBehaviour {
		public bool On {
			get => this.OnObject.activeSelf;
			set {
				this.OnObject.SetActive(value);
				this.OffObject.SetActive(!value);
			}
		}

		public GameObject OffObject;
		public GameObject OnObject;
	}
}
