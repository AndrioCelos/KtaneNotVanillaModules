using System;
#if (!DEBUG)
using Assets.Scripts.Components.VennWire;
#endif
using NotVanillaModulesLib.TestModel;
using Object = UnityEngine.Object;

namespace NotVanillaModulesLib {
	/// <summary>Represents a space for a wire in the Not Complicated Wires module and allows configuration of the associated light and symbol.</summary>
	public abstract class ComplicatedWireSpace {
		/// <summary>Returns or sets a value indicating whether this wire space has no wire.</summary>
		/// <remarks>This property should only be set to <c>true</c>. Setting it to <c>false</c> afterward is not supported.</remarks>
		public abstract bool Empty { get; set; }
		/// <summary>Returns a value indicating whether the wire has been cut.</summary>
		public abstract bool Cut { get; }
		/// <summary>Returns or sets a value indicating whether the light is on.</summary>
		public abstract bool LightOn { get; set; }
		/// <summary>Returns or sets a value indicating whether a star symbol is present.</summary>
		public abstract bool HasSymbol { get; set; }
		/// <summary>Returns or sets a value indicating the colours of the wire.</summary>
		/// <remarks>Setting this property more than once or setting it when <see cref="Empty"/> is <c>true</c> is not supported.</remarks>
		public abstract ComplicatedWireColours Colours { get; set; }

		/// <summary>Handles the initial power-up of the module.</summary>
		public abstract void Activate();

		internal class TestSpace : ComplicatedWireSpace {
			private bool active;
			private readonly NotComplicatedWiresConnector module;

			public override bool Empty {
				get => !this.Model.IntactWire.activeSelf && !this.Model.CutWire.activeSelf;
				set => this.Model.IntactWire.gameObject.SetActive(!value);
			}

			public override bool Cut => this.Model.CutWire.activeSelf;

			private bool lightOn;
			public override bool LightOn {
				get => this.lightOn;
				set {
					this.lightOn = value;
					if (this.active) this.Model.LED.On = value;
				}
			}
			public override bool HasSymbol {
				get => this.Model.SymbolText.gameObject.activeSelf;
				set => this.Model.SymbolText.gameObject.SetActive(value);
			}
			private ComplicatedWireColours colours;
			public override ComplicatedWireColours Colours {
				get => this.colours;
				set {
					this.colours = value;
					foreach (var renderer in this.Model.WireRenderers) renderer.material = this.module.TestWireMaterials[(int) value - 1];
				}
			}

			public TestModelWireSpace Model { get; }

			public TestSpace(NotComplicatedWiresConnector module, TestModelWireSpace model) {
				this.module = module;
				this.Model = model;
			}

			public override void Activate() {
				this.active = true;
				this.Model.LED.On = this.lightOn;
			}
		}

#if (!DEBUG)
		internal class LiveSpace : ComplicatedWireSpace {
			private bool active;

			private bool empty;
			public override bool Empty {
				get => this.empty;
				set {
					if (value) {
						if (!this.empty) {
							this.VennSnippableWire.DestroyAllColors();
							Object.Destroy(this.VennSnippableWire.GetComponent<Selectable>());
							this.colours = 0;
						}
					} else {
						if (this.empty) throw new InvalidOperationException("Cannot refill an empty " + nameof(ComplicatedWireSpace) + ".");
					}
					this.empty = value;
				}
			}

			private bool lightOn;
			public override bool LightOn {
				get => this.lightOn;
				set {
					this.lightOn = value;
					if (this.active) {
						this.VennSnippableWire.LEDOff.SetActive(!value);
						this.VennSnippableWire.LEDOn.SetActive(value);
						this.VennSnippableWire.LEDGlow.SetActive(value);
					}
				}
			}
			public override bool HasSymbol {
				get => this.VennSnippableWire.SymbolText.activeSelf;
				set => this.VennSnippableWire.SymbolText.SetActive(value);
			}
			private ComplicatedWireColours colours;
			public override ComplicatedWireColours Colours {
				get => this.colours;
				set {
					if (this.colours != 0) throw new InvalidOperationException("Cannot set colours of " + nameof(ComplicatedWireSpace) + " after they have already been set.");
					if (value != 0) {
						this.colours = value;
						this.VennSnippableWire.SetColor(value switch {
							ComplicatedWireColours.White => VennWireColor.White,
							ComplicatedWireColours.Red => VennWireColor.Red,
							ComplicatedWireColours.Blue => VennWireColor.Blue,
							ComplicatedWireColours.WhiteRed => VennWireColor.White | VennWireColor.Red,
							ComplicatedWireColours.WhiteBlue => VennWireColor.White | VennWireColor.Blue,
							ComplicatedWireColours.RedBlue => VennWireColor.Red | VennWireColor.Blue,
							_ => throw new ArgumentException()
						});
						this.VennSnippableWire.RemoveUnneededColors();
					}
				}
			}

			public override bool Cut => this.VennSnippableWire.Snipped;

			internal VennSnippableWire VennSnippableWire { get; private set; }

			public LiveSpace(VennSnippableWire vennSnippableWire) {
				this.VennSnippableWire = vennSnippableWire;
			}

			public override void Activate() {
				this.active = true;
				this.VennSnippableWire.LEDOff.SetActive(!this.lightOn);
				this.VennSnippableWire.LEDOn.SetActive(this.lightOn);
				this.VennSnippableWire.LEDGlow.SetActive(this.lightOn);
			}
		}
#endif
	}

	/// <summary>Represents a set of colours that can be set to a wire in the Not Complicated Wires module.</summary>
	public enum ComplicatedWireColours {
		None,
		White,
		Red,
		Blue,
		WhiteRed,
		WhiteBlue,
		RedBlue
	}
}
