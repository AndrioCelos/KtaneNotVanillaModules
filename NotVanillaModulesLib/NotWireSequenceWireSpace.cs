using System;
using NotVanillaModulesLib.TestModel;

namespace NotVanillaModulesLib {
	public abstract class NotWireSequenceWireSpace {
		public abstract WireSequenceColour Colour { get; set; }
		public int Index { get; set; }
		public abstract int To { get; set; }
		public abstract bool Cut { get; set; }
		public abstract string Letter { get; set; }
		public abstract string Number { get; set; }

		protected NotWireSequenceWireSpace(int index) {
			this.Index = index;
		}
		
		internal class TestWireSpace : NotWireSequenceWireSpace {
			internal readonly TestModelWireSequenceWireSpace wire;

			public override WireSequenceColour Colour {
				get => this.wire.Colour;
				set => this.wire.Colour = value;
			}
			public override int To {
				get => this.wire.To;
				set => this.wire.To = value;
			}
			public override bool Cut {
				get => this.wire.Cut;
				set => this.wire.Cut = value;
			}
			public override string Letter {
				get => this.wire.LetterTextMesh.text;
				set => this.wire.LetterTextMesh.text = value;
			}
			public override string Number {
				get => this.wire.NumberTextMesh.text;
				set => this.wire.NumberTextMesh.text = value;
			}

			public TestWireSpace(TestModelWireSequenceWireSpace wire, int index) : base(index) => this.wire = wire;
		}

#if (!DEBUG)
		internal class LiveWireSpace : NotWireSequenceWireSpace {
			private NotWireSequencePage.LiveNotWireSequencePage page;
			internal WireSequenceWire Wire { get; private set; }
			private WireSequenceColour colour;
			private int to;
			private string letter;
			private string number;

			public override WireSequenceColour Colour {
				get => this.colour;
				set { if (this.Wire != null) throw new InvalidOperationException("Cannot set this property after the wire is initialised."); this.colour = value; }
			}
			public override int To {
				get => this.to;
				set { if (this.Wire != null) throw new InvalidOperationException("Cannot set this property after the wire is initialised."); this.to = value; }
			}
			public override bool Cut {
				get => this.Wire != null && this.Wire.Snipped;
				set {
					if (this.Wire == null) throw new InvalidOperationException("Cannot set this property before the wire is initialised.");
					if (this.Wire.Snipped && !value) throw new ArgumentException("Cannot set " + nameof(Cut) + " to false.");
					this.Wire.Snipped = value;
				}
			}
			public override string Letter {
				get => this.letter;
				set { if (this.Wire != null) throw new InvalidOperationException("Cannot set this property after the wire is initialised."); this.letter = value; }
			}
			public override string Number {
				get => this.number;
				set { if (this.Wire != null) throw new InvalidOperationException("Cannot set this property after the wire is initialised."); this.number = value; }
			}

			public LiveWireSpace(NotWireSequencePage.LiveNotWireSequencePage page, int index) : base(index) =>
				this.page = page ?? throw new ArgumentNullException(nameof(page));

			public void InitialiseWire(WireSequenceWire wire) {
				if (this.Wire != null) throw new InvalidOperationException("The wire has already been initialised.");
				this.Wire = wire;
				wire.WireIndex = this.Index;
				wire.OnSnippedEvent = null;  // Handled using ParentComponent. We don't want the game's handler either.
			}
		}
#endif
	}

}
