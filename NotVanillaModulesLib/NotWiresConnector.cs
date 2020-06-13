using System;
using System.Collections.ObjectModel;
using NotVanillaModulesLib.TestModel;
using UnityEngine;

namespace NotVanillaModulesLib {
	public class NotWiresConnector : NotVanillaModuleConnector {
		public TestModelWireSpace[] TestModelWireSpaces;
		public Material[] Materials;
		public Material[] ColourblindMaterials;

		public ReadOnlyCollection<NotWireSpace> Wires { get; private set; }

		public event EventHandler<WireCutEventArgs> WireCut;

		protected override void AwakeLive() {
#if (!DEBUG)
			using var wrapper = this.InstantiateComponent<WireSetComponent>();
			var eventConnector = new WireEventConnector();
			eventConnector.WireCut += (sender, e) => this.WireCut?.Invoke(this, e);
			var wires = new NotWireSpace[wrapper.Component.wires.Count];
			for (int i = 0; i < wires.Length; ++i) {
				var liveWireSpace = new NotWireSpace.LiveWireSpace(this, wrapper.Component.wires[i], i);
				wires[i] = liveWireSpace;
				eventConnector.Attach(wrapper.Component.wires[i]);
			}
			this.Wires = Array.AsReadOnly(wires);

			var model = wrapper.Component.transform.Find("Model");
			model.SetParent(this.transform, false);
			model.transform.localPosition = new Vector3(0.006f, 0, -0.018f);
			model.transform.localScale = new Vector3(-1, 1, 0.83f);
#endif
		}
		protected override void AwakeTest() {
			var wires = new NotWireSpace[this.TestModelWireSpaces.Length];
			for (int i = 0; i < wires.Length; ++i) {
				var testWireSpace = new NotWireSpace.TestWireSpace(this, this.TestModelWireSpaces[i], i);
				wires[i] = testWireSpace;
				this.TestModelWireSpaces[i].WireCut += (sender, e) => this.WireCut?.Invoke(this, e);
			}
			this.Wires = Array.AsReadOnly(wires);
		}
		protected override void StartLive() {
#if (!DEBUG)
			var selectable = this.GetComponent<Selectable>();
			for (int i = 0; i < this.Wires.Count; ++i) {
				var childSelectable = ((NotWireSpace.LiveWireSpace) this.Wires[i]).wire.GetComponent<Selectable>();
				selectable.Children[i] = childSelectable;
				childSelectable.Parent = selectable;
			}
#endif
		}
		protected override void StartTest() { }

		public override bool ColourblindMode {
			get => base.ColourblindMode;
			set {
				base.ColourblindMode = value;
				if (value) foreach (var wire in this.Wires) wire.SetColourblindMode();
			}
		}

		public void Update() {
			var log = false;
			if (Input.GetKeyDown(KeyCode.E)) { log = true; foreach (var material in this.ColourblindMaterials) material.mainTextureScale -= Vector2.up; }
			if (Input.GetKeyDown(KeyCode.R)) { log = true; foreach (var material in this.ColourblindMaterials) material.mainTextureScale += Vector2.up; }
			if (Input.GetKeyDown(KeyCode.D)) { log = true; foreach (var material in this.ColourblindMaterials) material.mainTextureOffset -= Vector2.up * 0.1f; }
			if (Input.GetKeyDown(KeyCode.F)) { log = true; foreach (var material in this.ColourblindMaterials) material.mainTextureOffset += Vector2.up * 0.1f; }
			if (log) this.Log("{0} {1}", this.ColourblindMaterials[2].mainTextureScale, this.ColourblindMaterials[2].mainTextureOffset);
		}

		public bool TwitchCut(int spaceIndex) {
			if (this.Wires[spaceIndex].Cut) return false;
			if (this.TestMode) TwitchExtensions.Click(this.TestModelWireSpaces[spaceIndex]);
#if (!DEBUG)
			else TwitchExtensions.Click(((NotWireSpace.LiveWireSpace) this.Wires[spaceIndex]).wire);
#endif
			return true;
		}
	}
}
