using System;
using UnityEngine;

namespace NotVanillaModulesLib.TestModel {
	/// <summary>Represents a space for a wire in the test model for Wires, Complicated Wires or Wire Sequence.</summary>
	public class TestModelWireSpace : MonoBehaviour {
		public GameObject IntactWire;
		public GameObject CutWire;
		public GameObject LightOff;
		public GameObject LightOn;
		public MeshRenderer[] WireRenderers;
		public GameObject SymbolText;
		public int Index;

		public event EventHandler<WireCutEventArgs> WireCut;

		public bool Cut {
			get => this.CutWire.activeSelf;
			set {
				this.IntactWire.SetActive(!value);
				this.CutWire.SetActive(value);
			}
		}

		public void Start() => this.GetComponent<KMSelectable>().OnInteract = this.KMSelectable_OnInteract;

		private bool KMSelectable_OnInteract() {
			if (!this.Cut) {
				this.IntactWire.SetActive(false);
				this.CutWire.SetActive(true);
				this.WireCut?.Invoke(this, new WireCutEventArgs(this.Index));
			}
			return false;
		}
	}
}
