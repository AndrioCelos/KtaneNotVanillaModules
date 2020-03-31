using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if (!DEBUG)
using TMPro;
#endif

namespace NotVanillaModulesLib {
	public class NotCapacitorConnector : NotVanillaModuleConnector {
		public KMSelectable TestModelLever;
		public Transform TestModelLeverPivot;
		public Transform TestModelDeathBarFill;
		public GameObject TestModelLight;

		public KMAudio KMAudio { get; private set; }

#if (DEBUG)
		private Transform testHarnessNeedyTimer;
		private TextMesh testModelDisplayText;
#else
		private NeedyComponent needyComponent;
		private SpringedSwitch springedSwitch;
		private DeathBar deathBar;
		private LightBulb lightBulb;
		private ToneGenerator toneGenerator;
		private TextMeshPro displayText;

		private float MinTone = 300, MaxTone = 450, ToneStartTime = 20;
		private double baseGain;
#endif

		public event EventHandler LeverPressed;
		public event EventHandler LeverReleased;

		protected override void AwakeLive() {
#if (!DEBUG)
			var modulePrefab = GetComponentPrefab<NeedyDischargeComponent>();
			this.MinTone = modulePrefab.MinTone;
			this.MaxTone = modulePrefab.MaxTone;
			this.ToneStartTime = modulePrefab.ToneStartTime;
			this.Log($"{this.MinTone} {this.MaxTone} {this.ToneStartTime}");
			foreach (var child in modulePrefab.transform.Cast<Transform>()) {
				if (child.name != "Component_Needy_Background" && child.name != "Component_Highlight" &&
					child.name != "NeedyTimer(Clone)") {
					Instantiate(child, this.transform);
				}
			}

			this.springedSwitch = this.GetComponentInChildren<SpringedSwitch>();
			this.springedSwitch.OnPush = () => this.LeverPressed?.Invoke(this, EventArgs.Empty);
			this.springedSwitch.OnRelease = () => this.LeverReleased?.Invoke(this, EventArgs.Empty);

			this.lightBulb = this.GetComponentInChildren<LightBulb>();
#endif
		}
		protected override void AwakeTest() { }
		public override void Start() {
			base.Start();
			this.KMAudio = this.GetComponent<KMAudio>();
#if (!DEBUG)
			this.needyComponent = this.GetComponent<NeedyComponent>();
			this.toneGenerator = this.gameObject.AddComponent<ToneGenerator>();
			this.baseGain = this.toneGenerator.gain;
#endif
		}
		protected override void StartLive() {
#if (!DEBUG)
			// The death bar gets created on DeathBarParent.Awake.
			this.deathBar = this.transform.Find("DeathBarParent(Clone)/DeathBar(Clone)").GetComponent<DeathBar>();
			this.deathBar.transform.parent.localPosition = new Vector3(-0.126f, -0.0013f, -0.047f);
			this.deathBar.transform.parent.localEulerAngles = Vector3.zero;

			var selectable = this.GetComponent<ModSelectable>();
			selectable.Children[0] = this.springedSwitch.GetComponent<Selectable>();
			selectable.Children[0].Parent = selectable;
#endif
		}
		protected override void StartTest() {
			this.TestModelLever.OnInteract = this.TestModelLever_Interact;
			this.TestModelLever.OnInteractEnded = this.TestModelLever_InteractEnded;
		}

#pragma warning disable CA1822 // Mark members as static
		public void Update() {
#if (!DEBUG)
			if (this.needyComponent.State == NeedyComponent.NeedyStateEnum.Running) {
				var toneDuration = this.needyComponent.CountdownTime - this.ToneStartTime;
				float ratio = this.needyComponent.TimeRemaining / toneDuration;
				if (ratio <= 1) {
					// ToneGenerator.SetVolume didn't seem to have any effect. I don't know why.
					// So I'm controlling the volume via ToneGenerator.gain instead.
					this.toneGenerator.gain = this.baseGain * (1 - ratio) / 4;
					this.toneGenerator.PlayFrequency(this.MaxTone - ratio * (this.MaxTone - this.MinTone));
				}
			}
#endif
		}
#pragma warning restore CA1822 // Mark members as static

		/// <summary>Causes the capacitor pop effects and stops the sound effect and timer. Does not handle strikes.</summary>
		public void Explode() {
#if (DEBUG)
			this.StartCoroutine(this.TerminateTestCoroutine());
#else
			this.GetComponentInChildren<Capacitor>()?.Explode();
			this.toneGenerator.StopTune();
			this.needyComponent.StopAllCoroutines();
			typeof(NeedyComponent).GetMethod("ChangeState", BindingFlags.NonPublic | BindingFlags.Instance)
				.Invoke(this.needyComponent, new object[] { NeedyComponent.NeedyStateEnum.Terminated });
#endif
		}
#if (DEBUG)
		private IEnumerator TerminateTestCoroutine() {
			// This will stop the needy from reactivating itself automatically,
			// so it will stay in cooldown indefinitely.
			// The 'Activate Needy Modules' button will still reactivate it.
			yield return new WaitForSeconds(1);
			foreach (var component in this.testHarnessNeedyTimer.GetComponents<MonoBehaviour>())
				component.StopAllCoroutines();
		}
#endif

		public void SetDisplay(int number) {
#if (DEBUG)
			if (this.testModelDisplayText == null) {
				// Replace the needy timer with our display.
				this.testHarnessNeedyTimer = this.transform.Find("NeedyTimer(Clone)");
				var display = this.testHarnessNeedyTimer.Find("BackingActive/Text");
				this.testModelDisplayText = Instantiate(display, display.position, display.rotation, display.parent).GetComponent<TextMesh>();
				display.gameObject.SetActive(false);
			}
			this.testModelDisplayText.gameObject.SetActive(true);
			this.testModelDisplayText.text = number.ToString("D2");
#else
			if (this.displayText == null) {
				// Replace the needy timer with our display.
				var display = this.transform.Find("NeedyTimer(Clone)/SevenSegText");
				this.displayText = Instantiate(display, display.position, display.rotation, display.parent).GetComponent<TextMeshPro>();
				display.gameObject.SetActive(false);
			}
			this.displayText.gameObject.SetActive(true);
			this.displayText.text = number.ToString("D2");
#endif
		}

		public void ClearDisplay() {
#if (DEBUG)
			this.testModelDisplayText?.gameObject?.SetActive(false);
#else
			this.displayText?.gameObject?.SetActive(false);
			this.toneGenerator.gain = 0;
#endif
		}

		/// <param name="ratio">The amount to fill the death bar between 0 and 1.</param>
		public void SetDeathBar(float ratio) {
			if (this.TestMode) this.TestModelDeathBarFill.localScale = new Vector3(1, 1, ratio);
#if (!DEBUG)
			else this.deathBar.Value = ratio;
#endif
		}

		private bool TestModelLever_Interact() {
			this.PressLever();
			this.LeverPressed?.Invoke(this, EventArgs.Empty);
			if (this.TestMode) {
				this.TestModelLever.AddInteractionPunch(0.75f);
				this.KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, this.TestModelLeverPivot.transform);
			}
			return false;
		}

		private void TestModelLever_InteractEnded() {
			this.ReleaseLever();
			this.LeverReleased?.Invoke(this, EventArgs.Empty);
			if (this.TestMode) this.KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, this.TestModelLeverPivot.transform);
		}

		private void PressLever() {
			if (this.TestMode) {
				this.StopAllCoroutines();
				this.StartCoroutine(this.PressLeverTestCoroutine());
			}
#if (!DEBUG)
			else this.springedSwitch.SetAnimation(SpringedSwitch.AnimationState.PushDown);
#endif
		}
		private IEnumerator PressLeverTestCoroutine() {
			while (this.TestModelLeverPivot.localEulerAngles.x > 315 || this.TestModelLeverPivot.localEulerAngles.x < 180) {
				this.TestModelLeverPivot.localEulerAngles -= new Vector3(6, 0, 0);
				yield return null;
			}
		}

		private void ReleaseLever() {
			if (this.TestMode) {
				this.StopAllCoroutines();
				this.StartCoroutine(this.ReleaseLeverTestCoroutine());
			}
#if (!DEBUG)
			else this.springedSwitch.SetAnimation(SpringedSwitch.AnimationState.Release);
#endif
		}
		private IEnumerator ReleaseLeverTestCoroutine() {
			while (this.TestModelLeverPivot.localEulerAngles.x < 45 || this.TestModelLeverPivot.localEulerAngles.x > 180) {
				this.TestModelLeverPivot.localEulerAngles += new Vector3(9, 0, 0);
				yield return null;
			}
		}

		public void SetLight(bool on) {
			if (this.TestMode) this.TestModelLight.SetActive(on);
#if (!DEBUG)
			else this.lightBulb.Brightness = on ? 1 : 0;
#endif
		}

		public void TwitchPress() {
			if (this.TestMode) TwitchExtensions.Press(this.TestModelLever);
#if (!DEBUG)
			else TwitchExtensions.Press(this.springedSwitch);
#endif
		}

		public void TwitchRelease() {
			if (this.TestMode) TwitchExtensions.Release(this.TestModelLever);
#if (!DEBUG)
			else TwitchExtensions.Release(this.springedSwitch);
#endif
		}
	}
}
