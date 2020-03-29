using System;
using NotVanillaModulesLib.TestModel;
using UnityEngine;

namespace NotVanillaModulesLib {
	public abstract class NotWireSpace {
		public int WireIndex { get; }
		public abstract WireColour Colour { get; set; }
		public abstract bool Cut { get; }

		internal NotWireSpace(int index) => this.WireIndex = index;

		internal class TestWireSpace : NotWireSpace {
			private readonly NotWiresConnector module;
			internal readonly TestModelWireSpace wire;

			private WireColour colour;
			public override WireColour Colour {
				get => this.colour;
				set {
					this.colour = value;
					foreach (var renderer in this.wire.WireRenderers) renderer.material = this.module.Materials[(int) value];
				}
			}
			public override bool Cut => this.wire.CutWire.activeSelf;

			public TestWireSpace(NotWiresConnector module, TestModelWireSpace wire, int index) : base(index) {
				this.module = module ?? throw new ArgumentNullException(nameof(module));
				this.wire = wire ?? throw new ArgumentNullException(nameof(wire));
			}
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
					switch (value) {
						case WireColour.Red: this.wire.SetColor(BombGame.WireColor.red); return;
						case WireColour.Yellow: this.wire.SetColor(BombGame.WireColor.yellow); return;
						case WireColour.Blue: this.wire.SetColor(BombGame.WireColor.blue); return;
						case WireColour.White: this.wire.SetColor(BombGame.WireColor.white); return;
						case WireColour.Black: this.wire.SetColor(BombGame.WireColor.black); return;
						default:
							this.wire.SetColor(BombGame.WireColor.white);

							var renderer = this.wire.WireWhite.GetComponent<Renderer>();
							var texture = renderer.sharedMaterial.mainTexture;
							renderer.material = this.module.Materials[(int) value];
							InstanceDestroyer.AddObjectToDestroy(this.module.gameObject, renderer.material);
							renderer.material.mainTexture = texture;

							if (this.WireIndex == 5) {
								// Half of the sixth wire has materials assigned differently from every other segment.
								// I don't know why...
								this.wire.WireSnippedWhite.transform.GetChild(0).GetComponent<Renderer>().material = renderer.material;
								var renderer2 = this.wire.WireSnippedWhite.transform.GetChild(1).GetComponent<Renderer>();
								renderer2.materials = new[] { renderer2.materials[0], renderer.material };
							} else {
								foreach (Transform transform in this.wire.WireSnippedWhite.transform)
									transform.GetComponent<Renderer>().material = renderer.material;
							}
							break;
					}
				}
			}

			public override bool Cut => this.wire.Snipped;
		}
#endif
	}
}
