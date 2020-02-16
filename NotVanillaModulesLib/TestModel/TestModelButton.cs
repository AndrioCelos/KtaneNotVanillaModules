using System;
using UnityEngine;

namespace NotVanillaModulesLib.TestModel {
	public class TestModelButton : MonoBehaviour {
		public TextMesh TextMesh;
		public int Index;

		public event EventHandler<KeypadButtonEventArgs> Pressed;

		public void Start() => this.GetComponent<KMSelectable>().OnInteract =
			() => { this.Pressed?.Invoke(this, new KeypadButtonEventArgs(this.Index)); return false; };
	}
}
