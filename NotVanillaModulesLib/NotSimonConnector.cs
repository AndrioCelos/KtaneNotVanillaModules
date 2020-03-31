using System;
using System.Linq;
using System.Reflection;
using NotVanillaModulesLib.TestModel;

namespace NotVanillaModulesLib {
	public class NotSimonConnector : NotVanillaModuleConnector {
		public TestModelSimonButton[] TestModelButtons;

		public event EventHandler<SimonButtonEventArgs> ButtonPressed;

#if (!DEBUG)
		private SimonButton[] buttons;
		private ToneGenerator toneGenerator;
#endif

		protected override void AwakeLive() {
#if (!DEBUG)
			using var wrapper = this.InstantiateComponent<SimonComponent>();
			this.buttons = wrapper.Component.buttons;
			foreach (var button in this.buttons) button.transform.SetParent(this.transform, false);
			wrapper.Component.transform.Find("Frame").SetParent(this.transform, false);

			var positions = this.buttons.Select(b => b.ButtonGO.transform.localPosition).ToList();
			this.buttons[(int) SimonButtons.Red].ButtonGO.transform.localPosition = positions[(int) SimonButtons.Blue];
			this.buttons[(int) SimonButtons.Blue].ButtonGO.transform.localPosition = positions[(int) SimonButtons.Red];
			this.buttons[(int) SimonButtons.Green].ButtonGO.transform.localPosition = positions[(int) SimonButtons.Yellow];
			this.buttons[(int) SimonButtons.Yellow].ButtonGO.transform.localPosition = positions[(int) SimonButtons.Green];

			var keypadEventConnector = new KeypadEventConnector();
			keypadEventConnector.ButtonPressed += this.KeypadEventConnector_ButtonPressed;
			keypadEventConnector.Attach(this.buttons);

			FixKeypadButtons(this.buttons);
#endif
		}

		private void KeypadEventConnector_ButtonPressed(object sender, KeypadButtonEventArgs e)
			=> this.ButtonPressed?.Invoke(this, new SimonButtonEventArgs((SimonButtons) e.ButtonIndex));

		protected override void AwakeTest() { }
		public override void Start() {
			base.Start();
#if (!DEBUG)
			this.toneGenerator = this.gameObject.AddComponent<ToneGenerator>();
#endif
		}
		protected override void StartLive() {
#if (!DEBUG)
			var selectable = this.GetComponent<ModSelectable>();
			for (int i = 0; i < this.buttons.Length; ++i) {
				var selectable1 = this.buttons[i].GetComponent<Selectable>();
				selectable.Children[i * 2 + 1] = selectable1;
				selectable1.Parent = selectable;
			}
#endif
		}
		protected override void StartTest() {
			foreach (var button in this.TestModelButtons)
				button.Pressed += (sender, e) => this.ButtonPressed?.Invoke(this, new SimonButtonEventArgs((SimonButtons) e.ButtonIndex));
		}

		public void FlashLight(SimonButtons button) {
			if (this.TestMode) this.TestModelButtons[(int) button].Glow();
#if (!DEBUG)
			else this.buttons[(int) button].Glow();
#endif
		}

		public void PlayTones(float duration, params float[] notes) {
#if (!DEBUG)
			typeof(ToneGenerator).GetField("cutoffTime", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this.toneGenerator, 0f);
			typeof(ToneGenerator).GetField("noteTime", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this.toneGenerator, duration);
			this.toneGenerator.PlayTune(notes);
#endif
		}

		public void TwitchPress(SimonButtons button) {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelButtons[(int) button]);
#if (!DEBUG)
			else TwitchExtensions.Click(this.buttons[(int) button]);
#endif
		}
	}
}
