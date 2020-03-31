using System;
using System.Linq;
using NotVanillaModulesLib.TestModel;
using UnityEngine;
#if (!DEBUG)
using TMPro;
#endif

namespace NotVanillaModulesLib {
	public class NotMorseCodeConnector : NotVanillaModuleConnector {
		public GameObject TestModelTunerSlider;
		public TextMesh TestModelTunerDisplay;
		public GameObject LightOff;
		public GameObject LightOn;
		public TestModelButton TestModelDownButton;
		public TestModelButton TestModelUpButton;
		public TestModelButton TestModelSubmitButton;

		public event EventHandler DownPressed;
		public event EventHandler UpPressed;
		public event EventHandler SubmitPressed;
		public event EventHandler SubmitReleased;

		private float freqMarkerCurrent;
#pragma warning disable IDE0044 // Add readonly modifier
		private float freqMarkerSpeed = 0.25f;
		private int freqMarkerStartFreq = 500;
		private int freqMarkerEndFreq = 600;
#pragma warning restore IDE0044 // Add readonly modifier

#if (!DEBUG)
		private TextMeshPro displayText;
		private KeypadButton[] buttons;

		private Transform freqMarker;
		private Vector3 freqMarkerStart;
		private Vector3 freqMarkerEnd;
#endif

		private float targetSliderPosition;

		public void Update() {
			var d = this.targetSliderPosition - this.freqMarkerCurrent;
			if (d != 0) {
				var dx = this.freqMarkerSpeed * Time.deltaTime;
				if (d > 0) {
					if (d < dx) this.freqMarkerCurrent = this.targetSliderPosition;
					else this.freqMarkerCurrent += dx;
				} else {
					if (d > -dx) this.freqMarkerCurrent = this.targetSliderPosition;
					else this.freqMarkerCurrent -= dx;
				}
				if (this.TestMode) {
					var position = this.TestModelTunerSlider.transform.localPosition;
					position.x = (this.freqMarkerCurrent - 0.5f) * 0.11f;
					this.TestModelTunerSlider.transform.localPosition = position;
				}
#if (!DEBUG)
				else this.freqMarker.localPosition = Vector3.Lerp(this.freqMarkerStart, this.freqMarkerEnd, this.freqMarkerCurrent);
#endif
			}
		}

		protected override void AwakeLive() {
#if (!DEBUG)
			using var wrapper = this.InstantiateComponent<MorseCodeComponent>();
			this.displayText = wrapper.Component.DisplayText;
			this.displayText.transform.SetParent(this.transform, false);

			var layout = wrapper.Component.transform.Find("Component_Morse");
			layout.SetParent(this.transform, false);
			wrapper.Component.TransmitButton.Text.text = "XT";

			this.LightOff = wrapper.Component.LEDUnlit;
			this.LightOn = wrapper.Component.LEDLit;

			this.buttons = new[] { wrapper.Component.DownButton, wrapper.Component.UpButton, wrapper.Component.TransmitButton };

			var keypadEventConnector = new KeypadEventConnector();
			keypadEventConnector.ButtonPressed += this.KeypadEventConnector_ButtonPressed;
			keypadEventConnector.ButtonReleased += this.KeypadEventConnector_ButtonReleased;
			keypadEventConnector.Attach(this.buttons);

			var marker = layout.Find("Freq_Marker").GetComponent<FreqMarker>();
			this.freqMarker = marker.transform;
			this.freqMarkerStart = marker.StartPoint.localPosition;
			this.freqMarkerEnd = marker.EndPoint.localPosition;
			this.freqMarkerStartFreq = marker.StartFreq;
			this.freqMarkerEndFreq = marker.EndFreq;
			this.freqMarkerSpeed = marker.Speed;
			Destroy(marker);  // FreqMarker doesn't do what we need it to and may throw exceptions in Update.
#endif
		}


#if (!DEBUG)
		private void KeypadEventConnector_ButtonPressed(object sender, KeypadButtonEventArgs e) {
			switch (e.ButtonIndex) {
				case 0: this.DownPressed?.Invoke(sender, EventArgs.Empty); break;
				case 1: this.UpPressed?.Invoke(sender, EventArgs.Empty); break;
				case 2: this.SubmitPressed?.Invoke(sender, EventArgs.Empty); e.SuppressAutomaticRelease = true; break;
			}
		}
		private void KeypadEventConnector_ButtonReleased(object sender, KeypadButtonEventArgs e) {
			if (e.ButtonIndex == 2) this.SubmitReleased?.Invoke(sender, EventArgs.Empty);
		}
#endif

		protected override void AwakeTest() { }
		protected override void StartLive() {
#if (!DEBUG)
			var selectable = this.GetComponent<ModSelectable>();
			for (int i = 0; i < selectable.Children.Length; ++i) {
				selectable.Children[i] = this.buttons[i].GetComponent<Selectable>();
				selectable.Children[i].Parent = selectable;
			}
			this.displayText.GetComponent<Renderer>().enabled = false;
#endif
		}
		protected override void StartTest() {
			this.TestModelDownButton.Pressed += (sender, e) => this.DownPressed?.Invoke(this, EventArgs.Empty);
			this.TestModelUpButton.Pressed += (sender, e) => this.UpPressed?.Invoke(this, EventArgs.Empty);
			this.TestModelSubmitButton.Pressed += (sender, e) => this.SubmitPressed?.Invoke(this, EventArgs.Empty);
			this.TestModelSubmitButton.Released += (sender, e) => this.SubmitReleased?.Invoke(this, EventArgs.Empty);
			this.TestModelTunerDisplay.GetComponent<Renderer>().enabled = false;
		}

		public void Activate() {
			if (this.TestMode) this.TestModelTunerDisplay.GetComponent<Renderer>().enabled = true;
#if (!DEBUG)
			else this.displayText.GetComponent<Renderer>().enabled = true;
#endif
		}

		public void SetSlider(float position) => this.targetSliderPosition = position;
		public void SetSlider(int frequencyFractionalPartKHz) =>
			this.SetSlider((float) (frequencyFractionalPartKHz - this.freqMarkerStartFreq) / (this.freqMarkerEndFreq - this.freqMarkerStartFreq));
		public void SetSliderImmediate(float position) {
			this.SetSliderImmediate(position);
			this.freqMarkerCurrent = this.targetSliderPosition + 0.001f;
		}
		public void SetSliderImmediate(int frequencyFractionalPartKHz) {
			this.SetSliderImmediate(frequencyFractionalPartKHz);
			this.freqMarkerCurrent = this.targetSliderPosition + 0.001f;
		}

		public void SetDisplay(string frequencyFractionalPart) {
			if (this.TestMode) this.TestModelTunerDisplay.text = $"3.{frequencyFractionalPart}\u00A0MHz";
#if (!DEBUG)
			else this.displayText.text = $"3.{frequencyFractionalPart} MHz";
#endif
		}

		public void SetLight(bool on) {
			this.LightOff.SetActive(!on);
			this.LightOn.SetActive(on);
		}

		public void TwitchMoveDown() {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelDownButton);
#if (!DEBUG)
			else TwitchExtensions.Click(this.buttons[0]);
#endif
		}
		public void TwitchMoveUp() {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelUpButton);
#if (!DEBUG)
			else TwitchExtensions.Click(this.buttons[1]);
#endif
		}
		public void TwitchSubmit() {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelSubmitButton);
#if (!DEBUG)
			else TwitchExtensions.Click(this.buttons[2]);
#endif
		}
		public void TwitchPressSubmit() {
			if (this.TestMode) TwitchExtensions.Press(this.TestModelSubmitButton);
#if (!DEBUG)
			else TwitchExtensions.Press(this.buttons[2]);
#endif
		}
		public void TwitchReleaseSubmit() {
			if (this.TestMode) TwitchExtensions.Release(this.TestModelSubmitButton);
#if (!DEBUG)
			else TwitchExtensions.Release(this.buttons[2]);
#endif
		}
	}
}
