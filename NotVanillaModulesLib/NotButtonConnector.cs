using System;
using System.Collections;
using System.Linq;

#if (!DEBUG)
using TMPro;
#endif

using UnityEngine;

namespace NotVanillaModulesLib {
	public class NotButtonConnector : NotVanillaModuleConnector {
		public Transform TestModelCover;
		public MeshRenderer TestModelCap;
		public TextMesh TestModelText;
		public MeshRenderer LightRenderer;
		public TextMesh TestModelColourblindLightText;

		public Material[] Materials;
		public Material[] ColourblindMaterials;
		public Material[] LightMaterials;

		public KMAudio KMAudio { get; private set; }
		
		private KMSelectable testModelButton;
#if (!DEBUG)
		private PressableButton button;
		private Animator lidAnimator;
		private bool buttonBeingPushed;
		public TextMeshPro ColourblindLightText;
#endif

		public event EventHandler Held;
		public event EventHandler Released;

		private static readonly Color DarkTextColor = new Color(0, 0, 0, 0.8f);
		private static readonly Color LightTextColor = new Color(1, 1, 1, 0.9f);

		/// <summary>Returns a value indicating whether the vanilla Button module will open the cover on selection or focus. The value is not valid during Awake.</summary>
		public bool ShouldOpenCoverOnSelection { get; private set; }

		private ButtonColour colour;
		private ButtonLightColour lightColour;

		protected override void AwakeLive() {
#if (!DEBUG)
			var buttonComponentPrefab = GetComponentPrefab<ButtonComponent>();
			this.ShouldOpenCoverOnSelection = buttonComponentPrefab.LidBehaviour == 0 || KTInputManager.Instance.IsMotionControlMode();
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

			var text = Instantiate(GetComponentPrefab<PasswordComponent>().transform.Find("Layout_DEFAULT").GetComponent<PasswordLayout>().Spinners[0].Display, this.LightRenderer.transform.parent, false);
			this.ColourblindLightText = text;
			text.enableAutoSizing = false;
			text.transform.localPosition = new Vector3(0, 0.001f, 0);
			text.transform.localScale = new Vector3(0.005f, 0.005f, 1);
			text.alignment = TextAlignmentOptions.Center;
			text.color = new Color(0, 0, 0, 0.8f);
			text.lineSpacing = -12;
#endif
		}

		protected override void AwakeTest() { }

		public override void Start() {
			base.Start();
			this.KMAudio = this.GetComponent<KMAudio>();
		}

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
			this.testModelButton.OnInteract = this.TestModelButton_Interact;
			this.testModelButton.OnInteractEnded = this.TestModelButton_InteractEnded;
			this.TestModelColourblindLightText.gameObject.SetActive(false);
		}

		public override bool ColourblindMode {
			get => base.ColourblindMode;
			set {
				base.ColourblindMode = value;
				this.SetColour(this.colour);
				this.SetLightColour(this.lightColour);
			}
		}

		private void ButtonEventConnector_Held(object sender, EventArgs e) {
#if (!DEBUG)
			this.buttonBeingPushed = true;
#endif
			this.Held?.Invoke(this, e);
		}

		private void ButtonEventConnector_Released(object sender, EventArgs e) => this.Released?.Invoke(this, e);

		private bool TestModelButton_Interact() {
			this.Held?.Invoke(this, EventArgs.Empty);
			this.KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, this.testModelButton.transform);
			return false;
		}
		private void TestModelButton_InteractEnded() {
			this.Released?.Invoke(this, EventArgs.Empty);
			this.KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, this.testModelButton.transform);
		}

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
			this.colour = colour;
			var material = (this.ColourblindMode ? this.ColourblindMaterials : this.Materials)[(int) colour];
			var lightText = colour switch {
				ButtonColour.Yellow => false,
				ButtonColour.Cyan => false,
				ButtonColour.Pink => false,
				ButtonColour.White => false,
				_ => true
			};

			if (this.TestMode) {
				this.TestModelCap.material = material;
				this.TestModelText.color = lightText ? LightTextColor : DarkTextColor;
				// Colourblind materials use a high texture scale so that they will appear correctly on the vanilla button model.
				// This needs to be changed in the test harness.
				material.mainTextureScale = Vector2.one;
			} else {
#if (!DEBUG)
				var buttonComponent = this.button.GetComponent<PressableButton>();
				if (!lightText) buttonComponent.text.GetComponent<Renderer>().enabled = true;
				if (!this.ColourblindMode) {
					switch (colour) {
						case ButtonColour.Red: buttonComponent.SetColor(BombGame.ButtonColor.red); return;
						case ButtonColour.Yellow: buttonComponent.SetColor(BombGame.ButtonColor.yellow); return;
						case ButtonColour.Blue: buttonComponent.SetColor(BombGame.ButtonColor.blue); return;
						case ButtonColour.White: buttonComponent.SetColor(BombGame.ButtonColor.white); return;
						case ButtonColour.Green:
							buttonComponent.SetColor(BombGame.ButtonColor.red);
							buttonComponent.Button_Top_Red.SetActive(false);
							buttonComponent.Button_Top_Green.SetActive(true);
							return;
					}
				}
				buttonComponent.SetColor(BombGame.ButtonColor.white);
				var cap = this.button.transform.Find("ButtonTop").Find("Button_Top_White").GetComponent<MeshRenderer>();
				material = Instantiate(material);
				InstanceDestroyer.AddObjectToDestroy(this.gameObject, material);
				if (!this.ColourblindMode) material.mainTexture = cap.material.mainTexture;
				cap.material = material;
				buttonComponent.text.color = lightText ? LightTextColor : DarkTextColor;
				/*
					With light text, hide the text while the lights are off.
					If we don't do this, the text will appear lit even without the lights.
					The vanilla PressableButton does the same thing...
				*/
				buttonComponent.text.GetComponent<HideOnLightsChange>().enabled = lightText;
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

		public void SetLightColour(ButtonLightColour colour) {
			this.lightColour = colour;
			this.LightRenderer.material = this.LightMaterials[(int) colour];
			if (colour == ButtonLightColour.Off) {
				if (this.TestMode) this.TestModelColourblindLightText.gameObject.SetActive(false);
#if (!DEBUG)
				else this.ColourblindLightText.gameObject.SetActive(false);
#endif
			} else {
				var text = colour switch {
					ButtonLightColour.White => "W", ButtonLightColour.Red => "R", ButtonLightColour.Yellow => "Y",
					ButtonLightColour.Green => "G", ButtonLightColour.Blue => "B",
					ButtonLightColour.WhiteRed => "W\nR\nW\nR", ButtonLightColour.WhiteYellow => "W\nY\nW\nY",
					ButtonLightColour.WhiteGreen => "W\nG\nW\nG", ButtonLightColour.WhiteBlue => "W\nB\nW\nB",
					ButtonLightColour.RedYellow => "R\nY\nR\nY", ButtonLightColour.RedGreen => "R\nG\nR\nG", ButtonLightColour.RedBlue => "R\nB\nR\nB",
					ButtonLightColour.YellowGreen => "Y\nG\nY\nG", ButtonLightColour.YellowBlue => "Y\nB\nY\nB",
					ButtonLightColour.GreenBlue => "G\nB\nG\nB",
					_ => ""
				};
				if (this.TestMode) {
					this.TestModelColourblindLightText.text = text;
					this.TestModelColourblindLightText.gameObject.SetActive(this.ColourblindMode);
				}
#if (!DEBUG)
				else {
					this.ColourblindLightText.text = text;
					this.ColourblindLightText.gameObject.SetActive(this.ColourblindMode);
				}
#endif
				if (this.ColourblindMode) {
					this.LightRenderer.material.mainTextureScale = new Vector2(0.5f, 0.5f);
					this.LightRenderer.material.mainTextureOffset = new Vector2(0, 0.5f);
				}
			}
		}
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
