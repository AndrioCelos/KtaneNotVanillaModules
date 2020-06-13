using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NotVanillaModulesLib.TestModel;
#if (!DEBUG)
using TMPro;
using UnityEngine;
#endif

namespace NotVanillaModulesLib {
	public abstract class NotWireSequencePage {
		public abstract ReadOnlyCollection<NotWireSequenceWireSpace> Wires { get; }
		public abstract bool Active { get; set; }
		public abstract void SetColourblindMode();

		internal class TestNotWireSequencePage : NotWireSequencePage {
			private readonly TestModelWireSequencePage page;

			public override ReadOnlyCollection<NotWireSequenceWireSpace> Wires { get; }
			public override bool Active {
				get => this.page.gameObject.activeSelf;
				set => this.page.gameObject.SetActive(value);
			}

			public TestNotWireSequencePage(TestModelWireSequencePage page, int startIndex) {
				this.page = page ?? throw new ArgumentNullException(nameof(page));
				this.Wires = page.WireSpaces.Select(w => (NotWireSequenceWireSpace) new NotWireSequenceWireSpace.TestWireSpace(w, startIndex++)).ToList().AsReadOnly();
			}

			public override void SetColourblindMode() {
				foreach (var wire in this.Wires) wire.Colour = wire.Colour;  // Resets different materials.
			}
		}

#if (!DEBUG)
		internal class LiveNotWireSequencePage : NotWireSequencePage {
			private NotWireSequenceConnector module;
			internal WireSequencePage page;

			public override ReadOnlyCollection<NotWireSequenceWireSpace> Wires { get; }
			public override bool Active {
				get => this.page == null ? false : this.page.gameObject.activeSelf;
				set {
					if (this.page == null) throw new InvalidOperationException("Cannot set this property before the page is initialised.");
					this.page.gameObject.SetActive(value);
				}
			}

			public LiveNotWireSequencePage(NotWireSequenceConnector module, int startIndex) {
				this.module = module;
				this.Wires = Array.AsReadOnly(new NotWireSequenceWireSpace[] {
					new NotWireSequenceWireSpace.LiveWireSpace(this, startIndex),
					new NotWireSequenceWireSpace.LiveWireSpace(this, startIndex + 1),
					new NotWireSequenceWireSpace.LiveWireSpace(this, startIndex + 2)
				});
			}

			public void InitialisePage(WireSequencePage page, List<WireSequenceComponent.WireConfiguration> wireConfigurations, int pageIndex) {
				if (this.page != null) throw new InvalidOperationException("The page has already been initialised.");
				this.page = page;
				page.InitPage(pageIndex, 3, wireConfigurations, null);

				var wires = (IList<NotWireSequenceWireSpace>) this.Wires;
				for (int i = 0; i < wires.Count; i++) {
					var wire = (NotWireSequenceWireSpace.LiveWireSpace) wires[i];
					wire.InitialiseWire(page.Wires[i * 3 + wire.To]);
					if (this.module.ColourblindMode || wire.Colour == WireSequenceColour.Yellow || wire.Colour == WireSequenceColour.Green) {
						foreach (var renderer in wire.Wire.GetComponentsInChildren<Renderer>(true)) {
							if (renderer.gameObject.tag != "Highlight")
								renderer.material = (this.module.ColourblindMode ? this.module.ColourblindMaterials : this.module.Materials)[(int) wire.Colour];
						}
					}
				}

				page.OneText.text = this.Wires[0].Letter;
				page.TwoText.text = this.Wires[1].Letter;
				page.ThreeText.text = this.Wires[2].Letter;

				foreach (Transform transform in page.transform) {
					if (transform.gameObject.name[0] <= 'C') {
						// AText/BText/CText
						transform.localPosition = new Vector3(-0.0345f, transform.localPosition.y, transform.localPosition.z);
						transform.localScale = new Vector3(0.008f, 0.01f, 0.01f);
						var textMeshPro = transform.GetComponent<TextMeshPro>();
						textMeshPro.alignment = TextAlignmentOptions.Center;
						textMeshPro.text = this.Wires[transform.gameObject.name[0] - 'A'].Number;
					}
				}
			}

			public override void SetColourblindMode() {
				if (this.page == null) return;
				for (int i = 0; i < this.Wires.Count; i++) {
					var wire = (NotWireSequenceWireSpace.LiveWireSpace) this.Wires[i];
					foreach (var renderer in wire.Wire.GetComponentsInChildren<Renderer>(true)) {
						if (renderer.gameObject.tag != "Highlight")
							renderer.material = (this.module.ColourblindMode ? this.module.ColourblindMaterials : this.module.Materials)[(int) wire.Colour];
					}
				}
			}
		}
#endif
	}
}
