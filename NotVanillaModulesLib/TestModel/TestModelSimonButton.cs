using System.Collections;
using UnityEngine;

namespace NotVanillaModulesLib.TestModel {
	public class TestModelSimonButton : TestModelButton {
		public Material UnlitMaterial;
		public Material LitMaterial;

		public bool Lit {
			get => this.GetComponent<Light>().enabled;
			set {
				this.GetComponent<Light>().enabled = value;
				this.GetComponent<Renderer>().material = value ? this.LitMaterial : this.UnlitMaterial;
			}
		}

		public void Glow() {
			this.StopAllCoroutines();
			this.StartCoroutine(this.GlowCoroutine());
		}

		private IEnumerator GlowCoroutine() {
			this.Lit = true;
			yield return new WaitForSeconds(0.5f);
			this.Lit = false;
		}
	}
}
