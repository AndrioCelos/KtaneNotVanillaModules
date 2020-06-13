using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using NotVanillaModulesLib.TestModel;
using UnityEngine;
#if (!DEBUG)
using BombGame;
#endif

namespace NotVanillaModulesLib {
	public class NotWireSequenceConnector : NotVanillaModuleConnector {
		public Material[] Materials;
		public Material[] ColourblindMaterials;

		[Header("Test Model")]
		public Transform TestModelPanel;
		public Transform[] TestModelLidParts;
		public TestModelStageIndicator TestModelStageIndicator;
		public TestModelWireSequencePage TestModelPage;
		public KMSelectable TestModelUpButton;
		public KMSelectable TestModelDownButton;

		private bool testModelLidOpen;
#if (!DEBUG)
		private WireSequenceComponent component;
		private Animator animator;
#endif

		public event EventHandler<WireCutEventArgs> WireCut;
		public event EventHandler UpPressed;
		public event EventHandler DownPressed;
		public event EventHandler PanelSwitching;

		public bool Animating { get; private set; } = true;

		public ReadOnlyCollection<NotWireSequencePage> Pages { get; private set; }

		private int stage;
		public int Stage {
			get => this.stage;
			set {
				this.stage = value;
				if (this.TestMode) this.TestModelStageIndicator.Stage = value;
#if (!DEBUG)
				else this.component.StageIndicator.SetStage(value);
#endif
			}
		}

		public int CurrentPage { get; private set; }

		protected override void AwakeLive() {
#if (!DEBUG)
			this.GetComponent<ModBombComponent>().RequiresDeepBackingGeometry = true;

			using var wrapper = this.InstantiateComponent<WireSequenceComponent>();
			this.component = wrapper.Component;
			DestroyImmediate(wrapper.Component.StatusLightParent.gameObject);
			this.animator = wrapper.Component.GetComponent<Animator>();
			wrapper.Component.StageIndicator.NumStages = 4;

			wrapper.Component.UpButton.GetComponent<InteractiveObject>().OnInteract = o => this.UpPressed?.Invoke(this, EventArgs.Empty);
			wrapper.Component.DownButton.GetComponent<InteractiveObject>().OnInteract = o => this.DownPressed?.Invoke(this, EventArgs.Empty);

			var pages = new NotWireSequencePage[4];
			for (int i = 0; i < pages.Length; ++i) pages[i] = new NotWireSequencePage.LiveNotWireSequencePage(this, i * 3);
			this.Pages = Array.AsReadOnly(pages);

			// This time we need to keep the WireSequenceComponent instance's GameObject active,
			// so that the Animator will work.
			// The WireSequenceComponent instance itself must be removed first.
			// TempComponentWrapper.Dispose will not destroy the object if it is moved to a different parent.
			var transform = wrapper.Component.transform;
			DestroyImmediate(wrapper.Component.GetComponent<Selectable>());
			DestroyImmediate(wrapper.Component);
			transform.SetParent(this.transform, false);
#endif
		}
		protected override void AwakeTest() {
			void wireCut(object sender, WireCutEventArgs e) => this.WireCut?.Invoke(this, e);

			var pages = new NotWireSequencePage[4];
			pages[0] = new NotWireSequencePage.TestNotWireSequencePage(this.TestModelPage, 0);
			for (int i = 1; i < pages.Length; ++i) {
				var page = Instantiate(this.TestModelPage, this.TestModelPage.transform.parent);
				pages[i] = new NotWireSequencePage.TestNotWireSequencePage(page, i * 3);
				for (int j = 0; j < page.WireSpaces.Length; ++j) {
					page.WireSpaces[j].Index = i * 3 + j;
					page.WireSpaces[j].WireCut += wireCut;
				}
			}
			this.Pages = Array.AsReadOnly(pages);
			for (int i = 0; i < this.TestModelPage.WireSpaces.Length; ++i) {
				this.TestModelPage.WireSpaces[i].Index = i;
				this.TestModelPage.WireSpaces[i].WireCut += wireCut;
			}

			this.TestModelLidParts[0].localPosition = new Vector3(0, 0.031f, 0.0475f);
			this.TestModelLidParts[1].localPosition = new Vector3(0, 0.031f, -0.0475f);
			this.TestModelLidParts[0].localEulerAngles = Vector3.zero;
			this.TestModelLidParts[1].localEulerAngles = Vector3.zero;
			this.TestModelPanel.localPosition = new Vector3(0, 0.015f, 0);
		}
		protected override void StartLive() {
#if (!DEBUG)
			var selectable = this.GetComponent<ModSelectable>();
			selectable.Children[0] = this.component.UpButton;
			selectable.Children[0].Parent = selectable;
			selectable.Children[4] = this.component.DownButton;
			selectable.Children[4].Parent = selectable;
#endif
		}
		protected override void StartTest() {
			foreach (var page in this.Pages.Skip(1)) page.Active = false;  // Need to be active until now for TestSelectables to work properly.
			this.TestModelUpButton.OnInteract = () => { this.UpPressed?.Invoke(this, EventArgs.Empty); return false; };
			this.TestModelDownButton.OnInteract = () => { this.DownPressed?.Invoke(this, EventArgs.Empty); return false; };
		}

		public void InitialisePages() {
#if (!DEBUG)
			if (!this.TestMode) {
				// In test mode, this is done in AwakeTest.
				var wireConfigurations = new List<WireSequenceComponent.WireConfiguration>();
				for (int i = 0; i < 12; ++i) {
					var wire = this.Pages[i / 3].Wires[i % 3];
					wireConfigurations.Add(new WireSequenceComponent.WireConfiguration {
						Color = wire.Colour switch {
							WireSequenceColour.Black => WireColor.black,
							WireSequenceColour.Red => WireColor.red,
							WireSequenceColour.Blue => WireColor.blue,
							WireSequenceColour.Yellow => WireColor.yellow,
							_ => WireColor.white
						},
						To = wire.To
					});
				}
				var eventConnector = new WireEventConnector();
				eventConnector.WireCut += (sender, e) => this.WireCut?.Invoke(this, e);
				for (int i = 0; i < this.Pages.Count; ++i) {
					var page = (NotWireSequencePage.LiveNotWireSequencePage) this.Pages[i];
					var pageObject = Instantiate(this.component.PagePrefab, this.component.PageAnchor.transform);
					page.InitialisePage(pageObject, wireConfigurations, i);
					page.Active = false;
					foreach (NotWireSequenceWireSpace.LiveWireSpace wire in page.Wires) eventConnector.Attach(wire.Wire);
				}
			}
#endif
		}

		public override bool ColourblindMode {
			get => base.ColourblindMode;
			set {
				base.ColourblindMode = value;
				if (value) foreach (var page in this.Pages) page.SetColourblindMode();
			}
		}

		public void UpdateSelectables(int pageIndex) {
			var kmSelectable = this.GetComponent<KMSelectable>();
#if (!DEBUG)
			var selectable = this.GetComponent<Selectable>();
#endif

			if (this.TestMode) {
				for (int i = 0; i < 3; ++i) {
					var childKMSelectable = ((NotWireSequenceWireSpace.TestWireSpace) this.Pages[pageIndex].Wires[i]).wire.GetComponent<KMSelectable>();
					kmSelectable.Children[i + 1] = childKMSelectable;
					childKMSelectable.Parent = kmSelectable;
#if (!DEBUG)
					var childSelectable = ((NotWireSequenceWireSpace.TestWireSpace) this.Pages[pageIndex].Wires[i]).wire.GetComponent<Selectable>();
					childSelectable.Parent = selectable;
					childSelectable.y = i + 1;
#endif
				}
				kmSelectable.UpdateChildren();
			}
#if (!DEBUG)
			else {
				for (int i = 0; i < 3; ++i) {
					var childSelectable = ((NotWireSequenceWireSpace.LiveWireSpace) this.Pages[pageIndex].Wires[i]).Wire.GetComponent<Selectable>();
					selectable.Children[i + 1] = childSelectable;
					childSelectable.Parent = selectable;
					childSelectable.y = i + 1;
				}
			}
			selectable.Init();
			var currentSelectable = KTInputManager.Instance.GetCurrentSelectable();
			if ((currentSelectable != null && currentSelectable.Parent == selectable) || KTInputManager.Instance.IsMotionControlMode())
				selectable.OnDrillTo();
#else
			// The test model may be active in game, in which case there will be a ModSelectable instead of a TestSelectable.
			var testSelectable = this.GetComponent("TestSelectable");
			if (testSelectable != null) {
				testSelectable.GetType().GetMethod("Start", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(testSelectable, null);
			}
#endif
		}

		public void MoveToPage(int index) {
			this.GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSequenceMechanism, this.transform);
			this.StartCoroutine(this.AnimatePanelCoroutine(index));
		}

		private IEnumerator AnimatePanelCoroutine(int pageIndex) {
			this.Animating = true;
			if (this.TestMode) {
				float time;
				if (this.testModelLidOpen) {
					this.testModelLidOpen = false;
					time = 0;
					do {
						yield return null;
						time += Time.deltaTime;
						this.TestModelLidParts[0].localPosition = Vector3.Lerp(new Vector3(0, -0.014f, 0.0475f), new Vector3(0, 0.031f, 0.0475f), time / 0.25f);
						this.TestModelLidParts[1].localPosition = Vector3.Lerp(new Vector3(0, -0.014f, -0.0475f), new Vector3(0, 0.031f, -0.0475f), time / 0.25f);
						this.TestModelPanel.localPosition = Vector3.Lerp(new Vector3(0, 0.033f, 0), new Vector3(0, 0.015f, 0), time / 0.5f);
					} while (time < 0.25f);
					time = 0;
					do {
						yield return null;
						time += Time.deltaTime;
						this.TestModelLidParts[0].localEulerAngles = Vector3.Lerp(new Vector3(90, 0, 0), Vector3.zero, time / 0.25f);
						this.TestModelLidParts[1].localEulerAngles = Vector3.Lerp(new Vector3(-90, 0, 0), Vector3.zero, time / 0.25f);
						this.TestModelPanel.localPosition = Vector3.Lerp(new Vector3(0, 0.033f, 0), new Vector3(0, 0.015f, 0), (time + 0.25f) / 0.5f);
					} while (time < 0.25f);
					yield return new WaitForSeconds(0.2f);
				} else
					yield return new WaitForSeconds(0.7f);

				this.Pages[this.CurrentPage].Active = false;
				this.CurrentPage = pageIndex;
				this.PanelSwitching?.Invoke(this, EventArgs.Empty);

				if (this.CurrentPage >= 0 && this.CurrentPage < this.Pages.Count) {
					this.Pages[this.CurrentPage].Active = true;
					this.UpdateSelectables(this.CurrentPage);
					this.testModelLidOpen = true;
					time = 0;
					do {
						yield return null;
						time += Time.deltaTime;
						this.TestModelLidParts[0].localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(90, 0, 0), time / 0.25f);
						this.TestModelLidParts[1].localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(-90, 0, 0), time / 0.25f);
						this.TestModelPanel.localPosition = Vector3.Lerp(new Vector3(0, 0.015f, 0), new Vector3(0, 0.033f, 0), time / 0.5f);
					} while (time < 0.25f);
					time = 0;
					do {
						yield return null;
						time += Time.deltaTime;
						this.TestModelLidParts[0].localPosition = Vector3.Lerp(new Vector3(0, 0.031f, 0.0475f), new Vector3(0, -0.014f, 0.0475f), time / 0.25f);
						this.TestModelLidParts[1].localPosition = Vector3.Lerp(new Vector3(0, 0.031f, -0.0475f), new Vector3(0, -0.014f, -0.0475f), time / 0.25f);
						this.TestModelPanel.localPosition = Vector3.Lerp(new Vector3(0, 0.015f, 0), new Vector3(0, 0.033f, 0), (time + 0.25f) / 0.5f);
					} while (time < 0.25f);
				}
			}
#if (!DEBUG)
			else {
				this.animator.SetBool("Lower", true);
				yield return new WaitForSeconds(0.7f);
				this.Pages[this.CurrentPage].Active = false;
				this.CurrentPage = pageIndex;
				if (this.CurrentPage >= 0 && this.CurrentPage < this.Pages.Count) {
					this.Pages[this.CurrentPage].Active = true;
					this.UpdateSelectables(this.CurrentPage);
					this.PanelSwitching?.Invoke(this, EventArgs.Empty);
					this.animator.SetBool("Lower", false);
					yield return new WaitForSeconds(0.7f);
				}
			}
#endif
			this.Animating = false;
		}

		public void TwitchCut(int index) {
			if (this.TestMode) TwitchExtensions.Click(((NotWireSequenceWireSpace.TestWireSpace) this.Pages[this.CurrentPage].Wires[index]).wire);
#if (!DEBUG)
			else TwitchExtensions.Click(((NotWireSequenceWireSpace.LiveWireSpace) this.Pages[this.CurrentPage].Wires[index]).Wire);
#endif
		}
		public void TwitchMoveUp() {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelUpButton);
#if (!DEBUG)
			else TwitchExtensions.Click(this.component.UpButton);
#endif
		}
		public void TwitchMoveDown() {
			if (this.TestMode) TwitchExtensions.Click(this.TestModelDownButton);
#if (!DEBUG)
			else TwitchExtensions.Click(this.component.DownButton);
#endif
		}


	}
}
