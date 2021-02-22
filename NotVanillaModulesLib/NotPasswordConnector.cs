using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NotVanillaModulesLib.TestModel;
using UnityEngine;

namespace NotVanillaModulesLib {
	public class NotPasswordConnector : NotVanillaModuleConnector {
		public TestModelSpinner[] TestModelCharSpinners;
		public TestModelButton TestModelSubmitButton;

		public event EventHandler SubmitPressed;
		public event EventHandler SubmitReleased;

#if (!DEBUG)
		private IList<CharSpinner> spinners;
		private KeypadButton submitButton;
		private GameObject displayGlow;
		private PasswordLayout layout;
#endif

		protected override void AwakeLive() {
#if (!DEBUG)
			using var wrapper = this.InstantiateComponent<PasswordComponent>();

			// Remove the normal or the touch UI as the vanilla module does.
			if (typeof(PasswordComponent).GetField(nameof(PasswordComponent.TouchLayout), BindingFlags.Public | BindingFlags.Instance) != null) {
				if (KTInputManager.Instance.CurrentControlType == Assets.Scripts.Input.ControlType.Touch) {
					this.layout = GetTouchLayout(wrapper.Component);
					Destroy(GetDefaultLayout(wrapper.Component).gameObject);
				} else {
					this.layout = GetDefaultLayout(wrapper.Component);
					Destroy(GetTouchLayout(wrapper.Component).gameObject);
				}
			} else {
				// We're running on an old version of the game that lacks the touch layout.
				this.layout = wrapper.Component.transform.Find("Layout_DEFAULT").GetComponent<PasswordLayout>();
			}
			wrapper.Component.CurrentLayout = this.layout;
			this.layout.transform.SetParent(this.transform, false);
			this.spinners = wrapper.Component.Spinners;
			this.submitButton = wrapper.Component.SubmitButton;
			this.submitButton.transform.SetParent(this.transform, false);
			this.submitButton.transform.Rotate(new Vector3(0, 180, 0));
			this.displayGlow = wrapper.Component.DisplayGlow;
			this.displayGlow.transform.SetParent(this.transform, false);
			this.displayGlow.SetActive(false);

			var keypadEventConnector = new KeypadEventConnector();
			keypadEventConnector.ButtonPressed += this.KeypadEventConnector_ButtonPressed;
			keypadEventConnector.ButtonReleased += this.KeypadEventConnector_ButtonReleased;
			keypadEventConnector.Attach(this.submitButton);

			FixKeypadButtons(this.GetComponentsInChildren<KeypadButton>());
#endif
		}

#if (!DEBUG)
		// This needs to be wrapped into a method so that Awake doesn't throw a MissingFieldException on old versions of the game.
		private static PasswordLayout GetDefaultLayout(PasswordComponent module) => module.DefaultLayout;
		private static PasswordLayout GetTouchLayout(PasswordComponent module) => module.TouchLayout;

		private void KeypadEventConnector_ButtonPressed(object sender, KeypadButtonEventArgs e) {
			this.SubmitPressed?.Invoke(this, EventArgs.Empty);
			e.SuppressAutomaticRelease = true;
		}

		private void KeypadEventConnector_ButtonReleased(object sender, KeypadButtonEventArgs e) =>
			this.SubmitReleased?.Invoke(this, EventArgs.Empty);
#endif

		protected override void AwakeTest() {
		}
		protected override void StartLive() {
#if (!DEBUG)
			var selectable = this.GetComponent<ModSelectable>();
			for (int i = 0; i < 5; ++i) {
				selectable.Children[i] = this.spinners[i].UpButton.GetComponent<Selectable>();
				selectable.Children[i].Parent = selectable;
				selectable.Children[5 + i] = this.spinners[i].DownButton.GetComponent<Selectable>();
				selectable.Children[5 + i].Parent = selectable;
			}
			selectable.Children[12] = this.submitButton.GetComponent<Selectable>();
			selectable.Children[12].Parent = selectable;
#endif
		}
		protected override void StartTest() {
			this.TestModelSubmitButton.Pressed += (sender, e) => this.SubmitPressed?.Invoke(this, EventArgs.Empty);
			this.TestModelSubmitButton.Released += (sender, e) => this.SubmitReleased?.Invoke(this, EventArgs.Empty);
		}

		public void Activate() {
			if (this.TestMode) foreach (var spinner in this.TestModelCharSpinners) spinner.Activate();
#if (!DEBUG)
			else {
				this.layout.Activate();
				this.displayGlow.SetActive(true);
			}
#endif
		}

		public void SetSpinnerChoices(int index, IEnumerable<char> choices) {
			if (this.TestMode) this.TestModelCharSpinners[index].SetChoices(choices);
#if (!DEBUG)
			else {
				var charSpinner = this.spinners[index];
				charSpinner.Options = choices.ToList();
				charSpinner.UpdateDisplay();
			}
#endif
		}

		public void TwitchMoveUp(int index) {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelCharSpinners[index].UpButton);
#if (!DEBUG)
			else TwitchExtensions.Click(this.spinners[index].UpButton);
#endif
		}

		public void TwitchMoveDown(int index) {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelCharSpinners[index].DownButton);
#if (!DEBUG)
			else TwitchExtensions.Click(this.spinners[index].DownButton);
#endif
		}

		public void TwitchPressSubmit() {
			if (this.TestMode) TwitchExtensions.Press(this.TestModelSubmitButton);
#if (!DEBUG)
			else TwitchExtensions.Press(this.submitButton);
#endif
		}

		public void TwitchReleaseSubmit() {
			if (this.TestMode) TwitchExtensions.Release(this.TestModelSubmitButton);
#if (!DEBUG)
			else TwitchExtensions.Release(this.submitButton);
#endif
		}
	}
}
