using System;
using UnityEngine;

namespace NotVanillaModulesLib.TestModel {
	public class TestModelWireSequenceWireSpace : TestModelWireSpace {
		public Transform WireParent;
		public TextMesh LetterTextMesh;
		public TextMesh NumberTextMesh;
		public NotWireSequenceConnector Module;

		private int to;
		public int To {
			get => this.to;
			set {
				this.to = value;
				switch (value - this.Index % 3) {
					case -2:
						this.WireParent.localPosition = new Vector3(0, this.WireParent.localPosition.y, 0);
						this.WireParent.localEulerAngles = new Vector3(0, -52, 0);
						this.WireParent.localScale = new Vector3(1.5f, 1, 1);
						break;
					case -1:
						this.WireParent.localPosition = new Vector3(0, this.WireParent.localPosition.y, value == 0 ? 0.012f : -0.012f);
						this.WireParent.localEulerAngles = new Vector3(0, -33, 0);
						this.WireParent.localScale = new Vector3(1.2f, 1, 1);
						break;
					case 0:
						this.WireParent.localPosition = new Vector3(0, this.WireParent.localPosition.y, (1 - value) * 0.022f);
						this.WireParent.localRotation = Quaternion.identity;
						this.WireParent.localScale = Vector3.one;
						break;
					case 1:
						this.WireParent.localPosition = new Vector3(0, this.WireParent.localPosition.y, value == 1 ? 0.012f : -0.012f);
						this.WireParent.localEulerAngles = new Vector3(0, 33, 0);
						this.WireParent.localScale = new Vector3(1.2f, 1, 1);
						break;
					case 2:
						this.WireParent.localPosition = new Vector3(0, this.WireParent.localPosition.y, 0);
						this.WireParent.localEulerAngles = new Vector3(0, 52, 0);
						this.WireParent.localScale = new Vector3(1.5f, 1, 1);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private WireSequenceColour colour;
		public WireSequenceColour Colour {
			get => this.colour;
			set {
				this.colour = value;
				foreach (var renderer in this.WireRenderers) {
					renderer.material = (this.Module.ColourblindMode ? this.Module.ColourblindMaterials : this.Module.Materials)[(int) value];
					// Colourblind materials use a high texture scale so that they will appear correctly on the vanilla button model.
					// This needs to be changed in the test harness.
					renderer.material.mainTextureScale = new Vector2(1, 5);
					renderer.material.mainTextureOffset = Vector2.zero;
				}
			}
		}
	}
}
