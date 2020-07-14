using System;
using NotVanillaModulesLib.TestModel;
using UnityEngine;
using System.Collections;
#if (!DEBUG)
using BombGame;
using TMPro;
#endif

namespace NotVanillaModulesLib {
	/// <summary>A <see cref="Behaviour"/> that connects a mod module with the vanilla components for Knob.</summary>
	public class NotKnobConnector : NotVanillaModuleConnector {
		public TestModelKnob TestModelKnob;
		public Transform TestModelBase;
		public TestModelLED[] TestModelLEDs;

#if (!DEBUG)
		private PointingKnob knob;
		private NeedyKnobLED[] leds;
		private Rotater rotator;
#endif
		private readonly bool[] ledStates = new bool[12];
		private Coroutine panicCoroutine;
		private float currentRotation;
		private float targetRotation;

		public event EventHandler Turned;

		public bool Panicking { get; private set; }

		public KnobPosition Position {
			get {
#if (!DEBUG)
				if (!this.TestMode) return (KnobPosition) this.knob.CurrentRotation;
#endif
				return this.TestModelKnob.Position;
			}
		}

		internal static float KnobPositionToRotation(KnobPosition position) => position switch { KnobPosition.Up => 0, KnobPosition.Down => 180, KnobPosition.Left => 270, _ => 90 };

		protected override void AwakeLive() {
#if (!DEBUG)
			using var wrapper = this.InstantiateComponent<NeedyKnobComponent>();
			this.rotator = wrapper.Component.RotationBacking;
			var knobBase = this.rotator.transform.parent;
			knobBase.SetParent(this.transform, false);
			knobBase.GetComponentInChildren<TextMeshPro>().text = "U";
			this.knob = wrapper.Component.PointingKnob;
			this.leds = wrapper.Component.LEDs;
#endif
		}
		protected override void AwakeTest() { }
		protected override void StartLive() {
#if (!DEBUG)
			var selectable = this.GetComponent<ModSelectable>();
			selectable.Children[0] = this.knob.GetComponent<Selectable>();
			selectable.Children[0].Parent = selectable;
			selectable.Children[0].OnInteract += this.Knob_OnInteract;
#endif
		}

#if (!DEBUG)
		private bool Knob_OnInteract() {
			this.Turned?.Invoke(this, EventArgs.Empty);
			return false;
		}
#endif

		protected override void StartTest() {
			this.TestModelKnob.Turned += (sender, e) => this.Turned?.Invoke(this, e);
		}

		public void Update() {
			if (this.TestMode && this.currentRotation != this.targetRotation) {
				if (this.currentRotation < this.targetRotation) this.currentRotation += 3;
				else this.currentRotation -= 3;
				this.TestModelBase.localEulerAngles = new Vector3(0, this.currentRotation, 0);
			}
		}

		public void SetRotation(KnobPosition position) {
			if (this.TestMode) this.targetRotation = KnobPositionToRotation(position);
#if (!DEBUG)
			else this.rotator.TargetRotation = (Direction) position;
#endif
		}

		public void SetLEDs(bool[] ledStates) {
			ledStates.CopyTo(this.ledStates, 0);
			if (this.TestMode) {
				for (int i = this.TestModelLEDs.Length -  1; i >= 0; --i) this.TestModelLEDs[i].On = ledStates[i];
			}
#if (!DEBUG)
			else {
				for (int i = ledStates.Length - 1; i >= 0; --i) {
					if (ledStates[i]) {
						if (this.Panicking) {
							// If already panicking, StartPanic on other LEDs would make them flash out of phase unless we restart the routine.
							this.leds[i].StopPanic();
							this.leds[i].StartPanic();
						} else this.leds[i].SetPass();
					} else this.leds[i].SetInactive();
				}
			}
#endif
		}

		public void ClearLEDs() {
			this.Panicking = false;
			for (int i = this.ledStates.Length - 1; i >= 0; --i) this.ledStates[i] = false;
			if (this.TestMode) {
				if (this.panicCoroutine != null) {
					this.StopCoroutine(this.panicCoroutine);
					this.panicCoroutine = null;
				}
				foreach (var led in this.TestModelLEDs) led.On = false;
			}
#if (!DEBUG)
			else {
				foreach (var led in this.leds) led.SetInactive();
			}
#endif
		}

		public void PanicLEDs() {
			if (this.Panicking) return;
			this.Panicking = true;
			if (this.TestMode) {
				this.panicCoroutine = this.StartCoroutine(this.testPanic());
			}
#if (!DEBUG)
			else {
				for (int i = this.ledStates.Length - 1; i >= 0; --i) {
					if (this.ledStates[i]) this.leds[i].StartPanic();
				}
			}
#endif
		}
		private IEnumerator testPanic() {
			var state = true;
			while (true) {
				for (int i = this.ledStates.Length - 1; i >= 0; --i) {
					if (this.ledStates[i]) this.TestModelLEDs[i].On = state;
				}
				yield return new WaitForSeconds(0.1f);
				state = !state;
			}
		}

		public void TwitchTurn() {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelKnob);
#if (!DEBUG)
			else TwitchExtensions.Click(this.knob);
#endif
		}
	}
}
