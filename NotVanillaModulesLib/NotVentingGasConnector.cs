using System;
using NotVanillaModulesLib.TestModel;
using UnityEngine;
#if (!DEBUG)
using TMPro;
#endif

namespace NotVanillaModulesLib {
	public class NotVentingGasConnector : NotVanillaModuleConnector {
		public GameObject TestModelDisplayBase;
		public TextMesh[] TestModelDisplayTexts;
		public TestModelButton[] TestModelButtons;

#if (!DEBUG)
		private GameObject displayBase;
		private TextMeshPro[] displayTexts;
		private KeypadButton[] buttons;
#endif

		public bool DisplayActive {
			get {
#if (!DEBUG)
				if (!this.TestMode) return this.displayBase.activeSelf;
#endif
				return this.TestModelDisplayBase.activeSelf;
			}
			set {
#if (!DEBUG)
				if (!this.TestMode) this.displayBase.SetActive(value);
#endif
				this.TestModelDisplayBase.SetActive(value);
			}
		}

		public string DisplayText {
			get {
#if (!DEBUG)
				if (!this.TestMode) return this.displayTexts[0].text;
#endif
				return this.TestModelDisplayTexts[0].text;
			}
			set {
				if (this.TestMode) this.TestModelDisplayTexts[0].text = value;
#if (!DEBUG)
				else this.displayTexts[0].text = value;
#endif
			}
		}

		public string InputText {
			get {
#if (!DEBUG)
				if (!this.TestMode) return this.displayTexts[2].text;
#endif
				return this.TestModelDisplayTexts[2].text;
			}
			set {
				if (this.TestMode) this.TestModelDisplayTexts[2].text = value;
#if (!DEBUG)
				else this.displayTexts[2].text = value;
#endif
			}
		}

		public event EventHandler<VentingGasButtonEventArgs> ButtonPressed;

		protected override void AwakeLive() {
#if (!DEBUG)
			using var wrapper = this.InstantiateComponent<NeedyVentComponent>();
			wrapper.Component.transform.Find("Component_Needy_VentGas").SetParent(this.transform, false);

			this.displayBase = wrapper.Component.VentText;
			this.displayTexts = new[] {
				wrapper.Component.VentText.transform.Find("VentGas").GetComponent<TextMeshPro>(),
				wrapper.Component.VentText.transform.Find("VentYN").GetComponent<TextMeshPro>(),
				wrapper.Component.InputText };
			foreach (var text in this.displayTexts) DestroyImmediate(text.GetComponent<I2.Loc.Localize>());
			this.displayTexts[1].text = "N/Y";
			this.displayBase.gameObject.SetActive(false);
			this.displayBase.transform.SetParent(this.transform, false);
			wrapper.Component.InputText.transform.SetParent(this.transform, false);
			wrapper.Component.InputText.transform.SetParent(this.displayBase.transform, true);

			var keypadEventConnector = new KeypadEventConnector();
			keypadEventConnector.ButtonPressed += (sender, e) => this.ButtonPressed?.Invoke(this, new VentingGasButtonEventArgs((VentingGasButton) e.ButtonIndex));

			wrapper.Component.YesButton.gameObject.name = "Key_N";
			wrapper.Component.YesButton.GetComponentInChildren<TextMeshPro>().text = "N";
			wrapper.Component.NoButton.gameObject.name = "Key_Y";
			wrapper.Component.NoButton.GetComponentInChildren<TextMeshPro>().text = "Y";

			this.buttons = new[] { wrapper.Component.YesButton, wrapper.Component.NoButton };
			keypadEventConnector.Attach(this.buttons);

			FixKeypadButtons(this.buttons);
#endif
		}
		protected override void AwakeTest() { }
		protected override void StartLive() {
#if (!DEBUG)
			var selectable = this.GetComponent<ModSelectable>();
			for (int i = 0; i < this.buttons.Length; ++i) {
				selectable.Children[i] = this.buttons[i].GetComponent<Selectable>();
				selectable.Children[i].Parent = selectable;
			}
#endif
		}
		protected override void StartTest() {
			foreach (var button in this.TestModelButtons)
				button.Pressed += (sender, e) => this.ButtonPressed?.Invoke(this, new VentingGasButtonEventArgs((VentingGasButton) e.ButtonIndex));
			this.TestModelDisplayBase.gameObject.SetActive(false);
		}

		public void TwitchPress(VentingGasButton button) {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelButtons[(int) button]);
#if (!DEBUG)
			else TwitchExtensions.Click(this.buttons[(int) button]);
#endif
		}
	}
}
