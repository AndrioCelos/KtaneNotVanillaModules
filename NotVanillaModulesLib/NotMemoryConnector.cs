using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using NotVanillaModulesLib.TestModel;
#if (!DEBUG)
using TMPro;
#endif

namespace NotVanillaModulesLib {
	/// <summary>A <see cref="Behaviour"/> that connects a mod module with the vanilla components for Memory or Who's on First.</summary>
	public class NotMemoryConnector : NotVanillaModuleConnector {
		/// <summary>The type of module that should be instantiated.</summary>
		public ModuleType Module;
		public TestModelButton[] TestModelButtons;
		public TextMesh TestModelDisplayText;
		public TestModelStageIndicator TestModelStageIndicator;

		public event EventHandler<KeypadButtonEventArgs> ButtonPressed;
		public event EventHandler ButtonsSunk;
		public event EventHandler AnimationFinished;

#if (!DEBUG)
		private StageIndicatorBar stageIndicator;
		private TextMeshPro displayText;
		private IList<KeypadButton> buttons;
#endif

		public bool Animating { get; private set; }
		public bool InputValid { get; private set; }

		public string DisplayText {
			get {
#if (!DEBUG)
				if (!this.TestMode) return this.displayText.text;
#endif
				return this.TestModelDisplayText.text;
			}
			set {
#if (!DEBUG)
				if (!this.TestMode) { this.displayText.text = value; return; }
#endif
				this.TestModelDisplayText.text = value;
			}
		}

		private int stage;
		public int Stage {
			get => this.stage;
			set {
				this.stage = value;
				if (this.TestMode) this.TestModelStageIndicator.Stage = value;
#if (!DEBUG)
				else this.stageIndicator.SetStage(value);
#endif
			}
		}

		protected override void AwakeLive() {
#if (!DEBUG)
			if (this.Module == ModuleType.Memory) {
				using var wrapper = this.InstantiateComponent<MemoryComponent>();

				// Buttons need to be set before Awaking them.
				this.buttons = wrapper.Component.Buttons;
				foreach (var button in this.buttons) button.SetAnimationNoTween(KeypadButton.AnimationState.Sink);

				var childrenToKeep = (from Transform child in wrapper.Component.transform
									  where child.name != "StatusLightParent" && child.name != "Component_PuzzleBackground" && child.name != "Component_Highlight"
									  select child).ToList();
				foreach (var child in childrenToKeep) child.SetParent(this.transform, false);
				this.displayText = wrapper.Component.DisplayText;
				this.displayText.transform.localEulerAngles = new Vector3(90, 180, 0);
				this.stageIndicator = wrapper.Component.StageIndicator;
			} else {
				using var wrapper = this.InstantiateComponent<WhosOnFirstComponent>();

				// Buttons need to be set before Awaking them.
				this.buttons = wrapper.Component.Buttons;
				foreach (var button in this.buttons) button.SetAnimationNoTween(KeypadButton.AnimationState.Sink);

				wrapper.Component.transform.Find("Component_Whose").SetParent(this.transform, false);
				this.displayText = wrapper.Component.DisplayText;
				this.displayText.transform.SetParent(this.transform, false);
				this.displayText.GetComponent<Renderer>().enabled = false;

				var scale = this.displayText.transform.localScale;
				scale.x = -scale.x;
				this.displayText.transform.localScale = scale;

				this.buttons = wrapper.Component.Buttons;
				this.stageIndicator = wrapper.Component.StageIndicator;
				this.stageIndicator.NumStages = 5;
				this.stageIndicator.transform.SetParent(this.transform, false);
			}
			this.displayText.GetComponent<Renderer>().enabled = false;

			var keypadEventConnector = new KeypadEventConnector();
			keypadEventConnector.ButtonPressed += this.KeypadEventConnector_ButtonPressed;
			keypadEventConnector.Attach(this.buttons);
#endif
		}

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
			foreach (var button in this.TestModelButtons) {
				button.Pressed += (sender, e) => this.ButtonPressed?.Invoke(this, e);
				button.gameObject.SetActive(false);
			}
			this.TestModelDisplayText.gameObject.SetActive(false);
		}

		private void KeypadEventConnector_ButtonPressed(object sender, KeypadButtonEventArgs e)
			=> this.ButtonPressed?.Invoke(this, e);

		public void Activate() {
			this.SetDisplayOn(true);
			this.AnimateButtons();
		}

		public void SetButtonLabel(int index, string label) {
			if (this.TestMode) this.TestModelButtons[index].TextMesh.text = label;
#if (!DEBUG)
			else this.buttons[index].Text.text = label;
#endif
		}

		public void SetDisplayOn(bool on) {
			if (this.TestMode) this.TestModelDisplayText.gameObject.SetActive(on);
#if (!DEBUG)
			else this.displayText.GetComponent<Renderer>().enabled = on;
#endif
		}

		public void AnimateButtons() {
			this.StopAllCoroutines();
			this.StartCoroutine(this.AnimateButtonsCoroutine());
		}
		private IEnumerator AnimateButtonsCoroutine() {
			this.Animating = true;
			if (this.TestMode) {
				if (this.InputValid) {
					this.InputValid = false;
					this.SetDisplayOn(false);
					yield return new WaitForSeconds(0.75f);
					foreach (var button in this.TestModelButtons) {
						button.gameObject.SetActive(false);
						yield return new WaitForSeconds(0.05f);
					}
					yield return new WaitForSeconds(1);
					this.ButtonsSunk?.Invoke(this, EventArgs.Empty);
				}
				this.InputValid = true;
				foreach (var button in this.TestModelButtons) {
					button.gameObject.SetActive(true);
					yield return new WaitForSeconds(0.05f);
				}
#if (!DEBUG)
			} else {
				if (this.InputValid) {
					this.InputValid = false;
					this.SetDisplayOn(false);
					yield return new WaitForSeconds(0.75f);
					foreach (var button in this.buttons) {
						button.SetAnimation(KeypadButton.AnimationState.Sink);
						yield return new WaitForSeconds(0.05f);
					}
					yield return new WaitForSeconds(1);
					this.ButtonsSunk?.Invoke(this, EventArgs.Empty);
				}
				this.InputValid = true;
				foreach (var button in this.buttons) {
					button.SetAnimation(KeypadButton.AnimationState.Emerge);
					yield return new WaitForSeconds(0.05f);
				}
#endif
			}
			yield return new WaitForSeconds(0.75f);
			this.SetDisplayOn(true);
			this.Animating = false;
			this.AnimationFinished?.Invoke(this, EventArgs.Empty);
		}

		public void TwitchPress(int buttonIndex) {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelButtons[buttonIndex]);
#if (!DEBUG)
			else TwitchExtensions.Click(this.buttons[buttonIndex]);
#endif
		}

		public enum ModuleType {
			Memory,
			WhosOnFirst
		}
	}
}
