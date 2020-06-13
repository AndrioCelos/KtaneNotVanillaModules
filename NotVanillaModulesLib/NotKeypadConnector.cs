using System;
using System.Collections.Generic;
using System.Linq;
using NotVanillaModulesLib.TestModel;

using UnityEngine;

#if (!DEBUG)
using TMPro;
#endif

namespace NotVanillaModulesLib {
	/// <summary>A <see cref="Behaviour"/> that connects a mod module with the vanilla components for Keypad.</summary>
	public class NotKeypadConnector : NotVanillaModuleConnector {
		public TestModelButton[] TestModelButtons;
		public TextMesh[] TestModelColourblindTexts;
		public Renderer[] LightRenderers;
		public Light[] Lights;
		private readonly Symbol[] symbols = new Symbol[4];

		public event EventHandler<KeypadButtonEventArgs> ButtonPressed;

		public Color[] Colors = new[] {
			Color.black, Color.red, new Color(1, 0.5f, 0), Color.yellow, Color.green, Color.cyan, Color.blue,
			new Color(0.5f, 0, 1), Color.magenta, new Color(1, 0.5f, 0.75f), new Color(0.5f, 0.25f, 0), Color.grey, Color.white
		};

#if (!DEBUG)
		private IList<KeypadButton> buttons;
		private Texture[] symbolTextures;
		private TextMeshPro[] colourblindTexts;
#endif

		protected override void AwakeLive() {
#if (!DEBUG)
			var modulePrefab = GetComponentPrefab<KeypadComponent>();
			this.symbolTextures = modulePrefab.SymbolTextures;
			var model = Instantiate(modulePrefab.transform.Find("Model"), this.transform);

			this.buttons = model.transform.Cast<Transform>().Select(t => t.GetComponent<KeypadButton>()).Where(b => b != null).ToList();

			var keypadEventConnector = new KeypadEventConnector();
			keypadEventConnector.ButtonPressed += this.KeypadEventConnector_ButtonPressed;
			keypadEventConnector.Attach(this.buttons);

			for (int i = 0; i < this.buttons.Count; ++i) {
				var button = this.buttons[i];
				button.SymbolImage.AddComponent<ExcludeFromTexturePack>();
				this.LightRenderers[i].transform.parent.SetParent(button.transform);
			}
			FixKeypadButtons(this.buttons);

			var textPrefab = GetComponentPrefab<PasswordComponent>().transform.Find("Layout_DEFAULT").GetComponent<PasswordLayout>().Spinners[0].Display;
			this.colourblindTexts = new TextMeshPro[this.buttons.Count];
			for (int i = 0; i < this.buttons.Count; ++i) {
				var text = Instantiate(textPrefab, this.LightRenderers[i].transform.parent, false);
				this.colourblindTexts[i] = text;
				text.enableAutoSizing = false;
				text.transform.localPosition = new Vector3(0, 0.001f, 0.001f);
				text.transform.localScale = new Vector3(0.005f, 0.005f, 1);
				text.alignment = TextAlignmentOptions.Center;
				text.color = new Color(0, 0, 0, 0.8f);
				text.lineSpacing = -12;
			}
			foreach (var text in this.TestModelColourblindTexts) text.gameObject.SetActive(false);
#endif
		}

		private void KeypadEventConnector_ButtonPressed(object sender, KeypadButtonEventArgs e)
			=> this.ButtonPressed?.Invoke(this, e);

		protected override void AwakeTest() { }
		protected override void StartLive() {
#if (!DEBUG)
			var selectable = this.GetComponent<ModSelectable>();
			for (int i = 0; i < this.buttons.Count; ++i) {
				selectable.Children[i] = this.buttons[i].GetComponent<Selectable>();
				selectable.Children[i].Parent = selectable;
			}
#endif
		}
		protected override void StartTest() {
			foreach (var button in this.TestModelButtons) button.Pressed += (sender, e) => this.ButtonPressed?.Invoke(this, e);
			foreach (var text in this.TestModelColourblindTexts) text.gameObject.SetActive(false);
		}

		public override bool ColourblindMode {
			get => base.ColourblindMode;
			set {
				base.ColourblindMode = value;
				foreach (var cube in this.LightRenderers)
					cube.transform.localScale = new Vector3(cube.transform.localScale.x, cube.transform.localScale.y, value ? 0.012f : 0.0064f);
			}
		}

		public void SetSymbol(int index, Symbol symbol) {
			this.symbols[index] = symbol;
			if (this.TestMode) {
				this.TestModelButtons[index].TextMesh.text = GetSymbolChar(symbol).ToString();
			} else {
#if (!DEBUG)
				var material = this.buttons[index].SymbolImage.GetComponent<Renderer>().material;  // Clones the material if it was not already.
				material.mainTexture = this.symbolTextures[(int) symbol];
				InstanceDestroyer.AddObjectToDestroy(this.gameObject, material);
#endif
			}
		}

		public Symbol GetSymbol(int index) => this.symbols[index];

		public void SetLightColour(int index, LightColour colour) => this.SetLightColour(index, colour, null);
		public void SetLightColour(int index, LightColour colour, string colourblindText) {
			var rgb = this.Colors[(int) colour];
			this.LightRenderers[index].material.color = rgb;
			this.Lights[index].color = rgb;
			this.Lights[index].enabled = colour != LightColour.Black;
			if (colour == LightColour.Black) {
				if (this.TestMode) this.TestModelColourblindTexts[index].gameObject.SetActive(false);
#if (!DEBUG)
				else this.colourblindTexts[index].gameObject.SetActive(false);
#endif
			} else if (this.ColourblindMode) {
				var text = colourblindText ?? colour switch {
					LightColour.Red => "R", LightColour.Orange => "O", LightColour.Yellow => "Y", LightColour.Green => "G",
					LightColour.Cyan => "C", LightColour.Blue => "B", LightColour.Purple => "P", LightColour.Magenta => "M",
					LightColour.Pink => "I", LightColour.Brown => "N", LightColour.Grey => "A", LightColour.White => "W",
					_ => ""
				};
				if (this.TestMode) {
					this.TestModelColourblindTexts[index].gameObject.SetActive(true);
					this.TestModelColourblindTexts[index].text = text;
				}
#if (!DEBUG)
				else {
					this.colourblindTexts[index].gameObject.SetActive(true);
					this.colourblindTexts[index].text = text;
				}
#endif
			}
		}
		public void SetLightColour(int index, Color color) {
			this.LightRenderers[index].material.color = color;
			this.Lights[index].color = color;
			this.Lights[index].enabled = color.maxColorComponent > 0;
			if (this.TestMode) this.TestModelColourblindTexts[index].gameObject.SetActive(false);
#if (!DEBUG)
			else this.colourblindTexts[index].gameObject.SetActive(false);
#endif
		}

		public void TwitchPress(int buttonIndex) {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelButtons[buttonIndex]);
#if (!DEBUG)
			else TwitchExtensions.Click(this.buttons[buttonIndex]);
#endif
		}

		public static char GetSymbolChar(Symbol symbol) => symbol switch {
			Symbol.Copyright    => '©',
			Symbol.FilledStar   => '★',
			Symbol.HollowStar   => '☆',
			Symbol.SmileyFace   => 'ټ',
			Symbol.DoubleK      => 'Җ',
			Symbol.Omega        => 'Ω',
			Symbol.SquidKnife   => 'Ѭ',
			Symbol.Pumpkin      => 'ѽ',
			Symbol.HookN        => 'ϗ',
			Symbol.Teepee       => 'ϫ',
			Symbol.Six          => 'Ϭ',
			Symbol.SquigglyN    => 'Ϟ',
			Symbol.AT           => 'Ѧ',
			Symbol.Ae           => 'æ',
			Symbol.MeltedThree  => 'Ԇ',
			Symbol.Euro         => 'Ӭ',
			Symbol.Circle       => '҈',
			Symbol.NWithHat     => 'Ҋ',
			Symbol.Dragon       => 'Ѯ',
			Symbol.QuestionMark => '¿',
			Symbol.Paragraph    => '¶',
			Symbol.RightC       => 'Ͼ',
			Symbol.LeftC        => 'Ͽ',
			Symbol.Pitchfork    => 'Ψ',
			Symbol.Tripod       => 'Ѫ',
			Symbol.Cursive      => 'Ҩ',
			Symbol.Tracks       => '҂',
			Symbol.Balloon      => 'Ϙ',
			Symbol.WeirdNose    => 'ζ',
			Symbol.UpsideDownY  => 'ƛ',
			Symbol.BT           => 'Ѣ',
			_ => throw new ArgumentException(nameof(symbol))
		};

		public enum Symbol {
			Copyright,
			FilledStar,
			HollowStar,
			SmileyFace,
			DoubleK,
			Omega,
			SquidKnife,
			Pumpkin,
			HookN,
			Teepee,
			Six,
			SquigglyN,
			AT,
			Ae,
			MeltedThree,
			Euro,
			Circle,
			NWithHat,
			Dragon,
			QuestionMark,
			Paragraph,
			RightC,
			LeftC,
			Pitchfork,
			Tripod,
			Cursive,
			Tracks,
			Balloon,
			WeirdNose,
			UpsideDownY,
			BT
		}

		public enum LightColour {
			Black,
			Red,
			Orange,
			Yellow,
			Green,
			Cyan,
			Blue,
			Purple,
			Magenta,
			Pink,
			Brown,
			Grey,
			White
		}
	}
}
