using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace NotVanillaModulesLib {
	public class NotButtonConnector : NotVanillaModuleConnector {
		public Transform TestModelCover;
		public MeshRenderer TestModelCap;
		public TextMesh TestModelText;
		public MeshRenderer LightRenderer;

		public Material[] Materials;
		public Material[] LightMaterials;

		private KMSelectable testModelButton;
#if (!DEBUG)
		private PressableButton button;
		private Animator lidAnimator;
		private bool buttonBeingPushed;
#endif

		public event EventHandler Held;
		public event EventHandler Released;

		private static readonly Color DarkTextColor = new Color(0, 0, 0, 0.8f);
		private static readonly Color LightTextColor = new Color(1, 1, 1, 0.9f);

		/// <summary>Returns a value indicating whether the vanilla Button module will open the cover on selection or focus. The value is not valid during Awake.</summary>
		public bool ShouldOpenCoverOnSelection { get; private set; }

		protected override void AwakeLive() {
#if (!DEBUG)
			var buttonComponentPrefab = GetComponentPrefab<ButtonComponent>();
			this.ShouldOpenCoverOnSelection = buttonComponentPrefab.LidBehaviour == 0;
			var buttonPrefab = buttonComponentPrefab.transform.Find("Button").GetComponent<PressableButton>();
			this.button = Instantiate(buttonPrefab, this.transform);
			this.lidAnimator = this.button.transform.Find("Opening_LID").GetComponent<Animator>();

			this.button.transform.localPosition = new Vector3(0.008f, 0.001f, -0.006f);
			this.button.transform.localScale = new Vector3(-0.9f, 0.9f, 0.9f);
			// We mirror the button in the x direction so that the very faint 'border' around the light
			// (which is part of the same model as the button base) doesn't go off the module surface.
			// We must also mirror the text to compensate for this.
			var textScale = this.button.text.transform.localScale;
			textScale.x = -textScale.x;
			this.button.text.transform.localScale = textScale;

			// Remove the vanilla LED since we're replacing it with our own.
			Destroy(this.button.transform.Find("parts/LED_Off").gameObject);

			var buttonEventConnector = new ButtonEventConnector();
			buttonEventConnector.Held += this.ButtonEventConnector_Held;
			buttonEventConnector.Released += this.ButtonEventConnector_Released;
			buttonEventConnector.Attach(this.button);
#endif
		}

		protected override void AwakeTest() { }

		protected override void StartLive() {
#if (!DEBUG)
			var selectable = this.GetComponent<ModSelectable>();
			selectable.Children[0] = this.button.GetComponent<Selectable>();
			selectable.Children[0].Parent = selectable;
			var testModelButton = this.GetComponent<KMSelectable>().Children[0];
			selectable.Children[0].OnHighlight = () => testModelButton.OnHighlight?.Invoke();
			selectable.Children[0].OnHighlightEnded = () => testModelButton.OnHighlightEnded?.Invoke();
#endif
		}

		protected override void StartTest() {
			this.testModelButton = this.GetComponent<KMSelectable>().Children[0];
			this.testModelButton.OnInteract = () => { this.Held?.Invoke(this, EventArgs.Empty); return false; };
			this.testModelButton.OnInteractEnded = () => this.Released?.Invoke(this, EventArgs.Empty);
		}

		private void ButtonEventConnector_Held(object sender, EventArgs e) {
#if (!DEBUG)
			this.buttonBeingPushed = true;
#endif
			this.Held?.Invoke(this, e);
		}

		private void ButtonEventConnector_Released(object sender, EventArgs e) => this.Released?.Invoke(this, e);

		public void OpenCover() {
			if (this.TestMode) {
				this.StopAllCoroutines();
				this.StartCoroutine(this.OpenCoverTestCoroutine());
			} else {
#if (!DEBUG)
				this.lidAnimator.SetBool("IsLidOpen", true);
#endif
			}
		}
		private IEnumerator OpenCoverTestCoroutine() {
			while (this.TestModelCover.localEulerAngles.x < 80) {
				this.TestModelCover.localEulerAngles += new Vector3(4, 0, 0);
				yield return null;
			}
		}

		public void CloseCover() {
			if (this.TestMode) {
				this.StopAllCoroutines();
				this.StartCoroutine(this.CloseCoverTestCoroutine());
			} else {
#if (!DEBUG)
				if (this.buttonBeingPushed) {
					// Interacting with the button causes the module's OnDefocus event to be called.
					// We don't want to close the cover in this particular case.
					this.buttonBeingPushed = false;
				} else {
					this.lidAnimator.SetBool("IsLidOpen", false);
				}
#endif
			}
		}
		private IEnumerator CloseCoverTestCoroutine() {
			while (this.TestModelCover.localEulerAngles.x > 1) {
				this.TestModelCover.localEulerAngles -= new Vector3(4, 0, 0);
				yield return null;
			}
			this.TestModelCover.localEulerAngles = Vector3.zero;
		}

		public void SetColour(ButtonColour colour) {
			var material = this.Materials[(int) colour];
			var textColour = colour switch {
				ButtonColour.Yellow => DarkTextColor,
				ButtonColour.Cyan => DarkTextColor,
				ButtonColour.Pink => DarkTextColor,
				ButtonColour.White => DarkTextColor,
				_ => LightTextColor
			};

			if (this.TestMode) {
				this.TestModelCap.material = material;
				this.TestModelText.color = textColour;
			} else {
#if (!DEBUG)
				var buttonComponent = this.button.GetComponent<PressableButton>();
				switch (colour) {
					case ButtonColour.Red: buttonComponent.SetColor(BombGame.ButtonColor.red); break;
					case ButtonColour.Yellow: buttonComponent.SetColor(BombGame.ButtonColor.yellow); break;
					case ButtonColour.Blue: buttonComponent.SetColor(BombGame.ButtonColor.blue); break;
					case ButtonColour.Green:
						buttonComponent.SetColor(BombGame.ButtonColor.red);
						buttonComponent.Button_Top_Red.SetActive(false);
						buttonComponent.Button_Top_Green.SetActive(true);
						break;
					default:
						buttonComponent.SetColor(BombGame.ButtonColor.white);
						var cap = this.button.transform.Find("ButtonTop").Find("Button_Top_White").GetComponent<MeshRenderer>();
						material.mainTexture = cap.material.mainTexture;
						cap.material = material;
						buttonComponent.text.color = textColour;
						if (textColour == LightTextColor) {
							/*
								With light text, hide the text while the lights are off.
								If we don't do this, the text will appear lit even without the lights.
								The vanilla PressableButton does the same thing...
							*/
							buttonComponent.text.GetComponent<HideOnLightsChange>().enabled = true;
						}
						break;
				}
#endif
			}
		}

		public void SetLabel(ButtonLabel label) {
			if (this.TestMode)
				this.TestModelText.text = label.ToString().ToUpper();
#if (!DEBUG)
			else
				this.button.GetComponent<PressableButton>().text.text = label.ToString().ToUpper();
#endif
		}

		public void SetLightColour(ButtonLightColour colour) =>
			this.LightRenderer.material = this.LightMaterials[(int) colour];
		public void SetLightBrightness(float brightness) =>
			this.LightRenderer.material.SetFloat("_Blend", brightness);

		public void TwitchPress() {
			if (this.TestMode) TwitchExtensions.Press(this.testModelButton);
#if (!DEBUG)
			else TwitchExtensions.Press(this.button);
#endif
		}

		public void TwitchRelease() {
			if (this.TestMode) TwitchExtensions.Release(this.testModelButton);
#if (!DEBUG)
			else TwitchExtensions.Release(this.button);
#endif
		}
	}
}
