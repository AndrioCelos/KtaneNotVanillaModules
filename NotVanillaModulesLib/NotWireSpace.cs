using System;
using System.Linq;

using NotVanillaModulesLib.TestModel;
using UnityEngine;

namespace NotVanillaModulesLib {
	public abstract class NotWireSpace {
		public int WireIndex { get; }
		public abstract WireColour Colour { get; set; }
		public abstract bool Cut { get; }

		internal NotWireSpace(int index) => this.WireIndex = index;
		public abstract void SetColourblindMode();

		internal class TestWireSpace : NotWireSpace {
			private readonly NotWiresConnector module;
			internal readonly TestModelWireSpace wire;

			private WireColour colour;
			public override WireColour Colour {
				get => this.colour;
				set {
					this.colour = value;
					foreach (var renderer in this.wire.WireRenderers) {
						renderer.material = (this.module.ColourblindMode ? this.module.ColourblindMaterials : this.module.Materials)[(int) value];
						// Colourblind materials use a high texture scale so that they will appear correctly on the vanilla button model.
						// This needs to be changed in the test harness.
						renderer.material.mainTextureScale = new Vector2(1, 5);
						renderer.material.mainTextureOffset = Vector2.zero;
					}
				}
			}
			public override bool Cut => this.wire.CutWire.activeSelf;

			public TestWireSpace(NotWiresConnector module, TestModelWireSpace wire, int index) : base(index) {
				this.module = module ?? throw new ArgumentNullException(nameof(module));
				this.wire = wire ?? throw new ArgumentNullException(nameof(wire));
			}

			public override void SetColourblindMode() => this.Colour = this.Colour;  // Resets different materials.
		}

#if (!DEBUG)
		internal class LiveWireSpace : NotWireSpace {
			private readonly NotWiresConnector module;
			internal readonly SnippableWire wire;
			private WireColour colour;

			public LiveWireSpace(NotWiresConnector module, SnippableWire wire, int index) : base(index) {
				this.module = module ?? throw new ArgumentNullException(nameof(module));
				this.wire = wire ?? throw new ArgumentNullException(nameof(wire));
				wire.WireIndex = index;
			}

			public override WireColour Colour {
				get => this.colour;
				set {
					this.colour = value;
					if (!this.module.ColourblindMode) {
						switch (value) {
							case WireColour.Red: this.wire.SetColor(BombGame.WireColor.red); return;
							case WireColour.Yellow: this.wire.SetColor(BombGame.WireColor.yellow); return;
							case WireColour.Blue: this.wire.SetColor(BombGame.WireColor.blue); return;
							case WireColour.White: this.wire.SetColor(BombGame.WireColor.white); return;
							case WireColour.Black: this.wire.SetColor(BombGame.WireColor.black); return;
						}
					}
					this.wire.SetColor(BombGame.WireColor.white);

					var renderer = this.wire.WireWhite.GetComponent<Renderer>();
					var texture = renderer.sharedMaterial.mainTexture;
					renderer.sharedMaterial = (this.module.ColourblindMode ? this.module.ColourblindMaterials : this.module.Materials)[(int) value];
					if (!this.module.ColourblindMode) {
						renderer.material.mainTexture = texture;
						InstanceDestroyer.AddObjectToDestroy(this.module.gameObject, renderer.material);
					}
					if (this.WireIndex == 5) {
						// Half of the sixth wire has materials assigned differently from every other segment.
						// I don't know why...
						this.wire.WireSnippedWhite.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = renderer.sharedMaterial;
						var renderer2 = this.wire.WireSnippedWhite.transform.GetChild(1).GetComponent<Renderer>();
						renderer2.sharedMaterials = new[] { renderer.sharedMaterial, renderer.sharedMaterial };
					} else {
						foreach (Transform transform in this.wire.WireSnippedWhite.transform)
							transform.GetComponent<Renderer>().sharedMaterial = renderer.sharedMaterial;
					}
				}
			}

			public override bool Cut => this.wire.Snipped;

			public override void SetColourblindMode() {
				var material = this.module.ColourblindMaterials[(int) this.Colour];
				foreach (var renderer in this.wire.GetComponentsInChildren<Renderer>(true)) {
					if (renderer.gameObject.tag != "Highlight") {
						renderer.sharedMaterial = material;
						if (this.wire.GetColor() == BombGame.WireColor.yellow) {
							// The texture is mapped differently on the yellow wire model.
							renderer.material.mainTextureScale = new Vector2(50, 10);
							renderer.material.mainTextureOffset = new Vector2(0, 0.2f);
							InstanceDestroyer.AddObjectToDestroy(renderer.gameObject, renderer.material);
						}
					}
				}
			}
		}
#endif
	}
}
