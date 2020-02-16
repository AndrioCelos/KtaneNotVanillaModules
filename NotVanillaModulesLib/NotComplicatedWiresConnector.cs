using System;
using System.Collections.ObjectModel;
using System.Linq;
using NotVanillaModulesLib.TestModel;
using UnityEngine;
#if (!DEBUG)
using Assets.Scripts.Components.VennWire;
#endif

namespace NotVanillaModulesLib {
	/// <summary>A <see cref="Behaviour"/> that connects a mod module with the vanilla components for Complicated Wires.</summary>
	public class NotComplicatedWiresConnector : NotVanillaModuleConnector {
		public TestModelWireSpace[] TestModelWireSpaces;
		public Material[] TestWireMaterials;
		public Material[] SocketMaterials;

		public ReadOnlyCollection<ComplicatedWireSpace> WireSpaces { get; private set; }

		public event EventHandler<WireCutEventArgs> WireCut;

		protected override void AwakeLive() {
#if (!DEBUG)
			// We need to instantiate the entire module prefab to copy references between its children properly.
			using var wrapper = this.InstantiateComponent<VennWireComponent>();
			var vennSnippableWires = wrapper.Component.Wires;

			var componentVennWires = wrapper.Component.transform.Find("Component_VennWires");
			componentVennWires.SetParent(this.transform, false);

			// Oh god, Complicated Wires.
			// The two socket arrays are actually a single model, so inverting them is really tricky.
			// The solution used here is to make two copies of the model that each render one half
			// using a custom shader.
			var ledSocketRenderer = componentVennWires.Find("group1").GetComponent<MeshRenderer>();
			ledSocketRenderer.name = "LED Sockets";
			this.SocketMaterials[0].mainTexture = ledSocketRenderer.material.mainTexture;
			ledSocketRenderer.material = this.SocketMaterials[0];

			var symbolSocketRenderer = Instantiate(ledSocketRenderer, ledSocketRenderer.transform.parent);
			symbolSocketRenderer.name = "Symbol Sockets";
			this.SocketMaterials[1].mainTexture = symbolSocketRenderer.material.mainTexture;
			symbolSocketRenderer.material = this.SocketMaterials[1];

			var childrenToKeep = (from Transform child in wrapper.Component.transform
								  where child != componentVennWires && child.name != "StatusLightParent" &&
									child.name != "Component_PuzzleBackground" && child.name != "Component_Highlight"
								  select child).ToList();
			foreach (var child in childrenToKeep) {
				child.SetParent(this.transform, false);
				if (child.name.StartsWith("LightGlow"))
					// We need to SetParent twice because the first call changes the object's world position.
					// The second call needs to preserve the new world position.
					child.SetParent(ledSocketRenderer.transform, true);
			}

			// Reparent LED and symbol text objects so that they can be moved with the models.
			foreach (var child in componentVennWires.transform.Cast<Transform>().Where(t => t.name.StartsWith("LED_")).ToList())
				child.parent = ledSocketRenderer.transform;
			componentVennWires.transform.Find("Booleans").parent = symbolSocketRenderer.transform;

			// Shift the models around.
			ledSocketRenderer.transform.localPosition = new Vector3(-0.018f, -0.009f, 0);
			ledSocketRenderer.transform.localScale = new Vector3(1.218f, 1.623f, -1);
			symbolSocketRenderer.transform.localPosition = new Vector3(0.013f, 0.007f, 0);
			symbolSocketRenderer.transform.localScale = new Vector3(0.82f, 0.582f, -1);

			var eventConnector = new WireEventConnector();
			eventConnector.WireCut += this.EventConnector_WireCut;
			foreach (var wire in vennSnippableWires) eventConnector.Attach(wire);

			var spaces = new ComplicatedWireSpace[vennSnippableWires.Length];
			for (int i = 0; i < vennSnippableWires.Length; ++i) {
				vennSnippableWires[i].WireIndex = i;
				spaces[i] = new ComplicatedWireSpace.LiveSpace(vennSnippableWires[i]);
			}
			this.WireSpaces = Array.AsReadOnly(spaces);
#endif
		}

		protected override void AwakeTest() {
			var spaces = new ComplicatedWireSpace[this.TestModelWireSpaces.Length];
			for (int i = 0; i < spaces.Length; ++i)
				spaces[i] = new ComplicatedWireSpace.TestSpace(this, this.TestModelWireSpaces[i]);
			this.WireSpaces = Array.AsReadOnly(spaces);
		}
		protected override void StartLive() { }
		protected override void StartTest() {
			foreach (var space in this.TestModelWireSpaces)
				space.WireCut += (sender, e) => this.WireCut?.Invoke(this, e);
		}

		private void EventConnector_WireCut(object sender, WireCutEventArgs e) => this.WireCut?.Invoke(this, e);

		/// <summary>Sets up selectables in the module after wire states have been initialised.</summary>
		public void UpdateSelectable() {
			if (this.TestMode) {
				var selectable = this.GetComponent<KMSelectable>();
				for (int i = 0; i < selectable.Children.Length; ++i) {
					if (this.WireSpaces[i].Empty || this.WireSpaces[i].Colours == 0)
						selectable.Children[i] = null;
					else {
						selectable.Children[i] = ((ComplicatedWireSpace.TestSpace) this.WireSpaces[i]).Model.GetComponent<KMSelectable>();
						selectable.Children[i].Parent = selectable;
					}
				}
				selectable.UpdateChildren();
			} else {
#if (!DEBUG)
				var activeWires = this.WireSpaces.Where(s => !s.Empty).ToList();
				var selectable = this.GetComponent<ModSelectable>();
				for (int i = 0; i < selectable.Children.Length; ++i) {
					if (this.WireSpaces[i].Empty || this.WireSpaces[i].Colours == 0)
						selectable.Children[i] = null;
					else {
						selectable.Children[i] = ((ComplicatedWireSpace.LiveSpace) this.WireSpaces[i]).VennSnippableWire.GetComponent<Selectable>();
						selectable.Children[i].Parent = selectable;
					}
				}
#endif
			}
		}

		public bool TwitchCut(int spaceIndex) {
			if (this.WireSpaces[spaceIndex].Empty || this.WireSpaces[spaceIndex].Cut) return false;
			if (this.TestMode)
				TwitchExtensions.Click(this.TestModelWireSpaces[spaceIndex]);
#if (!DEBUG)
			else
				TwitchExtensions.Click(((ComplicatedWireSpace.LiveSpace) this.WireSpaces[spaceIndex]).VennSnippableWire);
#endif
			return true;
		}
	}
}
