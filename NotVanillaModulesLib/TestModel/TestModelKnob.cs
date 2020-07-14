using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace NotVanillaModulesLib.TestModel {
	public class TestModelKnob : MonoBehaviour {
		public KnobPosition Position { get; private set; }

		public event EventHandler Turned;

		public void Start() {
			this.GetComponent<KMSelectable>().OnInteract += this.KMSelectable_OnInteract;
		}

		private bool KMSelectable_OnInteract() {
			this.Position = this.Position switch { KnobPosition.Up => KnobPosition.Right, KnobPosition.Right => KnobPosition.Down, KnobPosition.Down => KnobPosition.Left, _ => KnobPosition.Up };
			this.transform.localEulerAngles = new Vector3(0, NotKnobConnector.KnobPositionToRotation(this.Position), 0);
			this.Turned?.Invoke(this, EventArgs.Empty);
			return false;
		}
	}
}
