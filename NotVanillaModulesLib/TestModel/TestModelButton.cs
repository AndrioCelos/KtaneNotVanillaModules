using System;
using UnityEngine;

namespace NotVanillaModulesLib.TestModel {
	public class TestModelButton : MonoBehaviour {
		public TextMesh TextMesh;
		public int Index;
		public KMAudio KMAudio;
		private KMSelectable kmSelectable;

		public event EventHandler<KeypadButtonEventArgs> Pressed;
		public event EventHandler<KeypadButtonEventArgs> Released;

		public void Start() {
			this.kmSelectable = this.GetComponent<KMSelectable>();
			this.kmSelectable.OnInteract = this.KMSelectable_Interact;
			this.kmSelectable.OnInteractEnded = this.KMSelectable_InteractEnded;
		}

		private bool KMSelectable_Interact() {
			this.Pressed?.Invoke(this, new KeypadButtonEventArgs(this.Index));
			this.KMAudio?.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, this.transform);
			this.kmSelectable.AddInteractionPunch(0.75f);
			return false;
		}

		private void KMSelectable_InteractEnded() {
			this.Released?.Invoke(this, new KeypadButtonEventArgs(this.Index));
			this.KMAudio?.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, this.transform);
		}
	}
}