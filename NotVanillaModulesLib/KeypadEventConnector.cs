using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NotVanillaModulesLib {
#if (!DEBUG)
	/// <summary>Passes events from keypad buttons to a Not Vanilla module. This class is not available in debug builds.</summary>
	internal class KeypadEventConnector : IButtonParent {
		/// <summary>Called when a button attached to this instance is pressed.</summary>
		public event EventHandler<KeypadButtonEventArgs> ButtonPressed;
		/// <summary>Called when a button attached to this instance is released (automatically or manually).</summary>
		public event EventHandler<KeypadButtonEventArgs> ButtonReleased;

		private readonly List<KeypadButton> buttons = new List<KeypadButton>();

		/// <summary>Returns or sets a value indicating whether interactions with buttons attached to this instance should be disabled.</summary>
		/// <seealso cref="IButtonParent.IsLocked"/>
		public bool IsLocked { get; set; }

		/// <summary>Attaches this instance to the specified <see cref="KeypadButton"/>, allowing events to be received from the button.</summary>
		public void Attach(KeypadButton button) {
			button.ParentComponent = this;
			this.buttons.Add(button);
		}
		/// <summary>Attaches this instance to the specified <see cref="KeypadButton"/> and sets its <see cref="KeypadButton.ButtonIndex"/>.</summary>
		public void Attach(KeypadButton button, int index) {
			this.Attach(button);
			button.ButtonIndex = index;
		}
		/// <summary>Attaches this instance to all buttons in the specified collection and sets their indices accordingly.</summary>
		public void Attach(IEnumerable<KeypadButton> buttons) {
			int i = 0;
			foreach (var button in buttons) this.Attach(button, i++);
		}

		bool IButtonParent.ButtonDown(int index) {
			var e = new KeypadButtonEventArgs(index);
			this.ButtonPressed?.Invoke(this, e);

			if (e.SuppressAutomaticRelease) {
				// LeanTween.description(GameObject) isn't in the version used by the game...
				foreach (var tween in (LTDescr[]) typeof(LeanTween).GetField("tweens", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)) {
					KeypadButton button;
					if (tween != null && tween.toggle && (button = this.buttons.FirstOrDefault(b => b.transform == tween.trans)) != null && button.ButtonIndex == index) {
						tween.setOnComplete((Action) null);
						break;
					}
				}
			}

			return e.StayDown;
		}
		bool IButtonParent.ButtonUp(int index) {
			var e = new KeypadButtonEventArgs(index);
			this.ButtonReleased?.Invoke(this, e);
			return e.StayDown;  // It's never used, but we'll return it anyway.
		}
		bool IButtonParent.IsLocked() => this.IsLocked;
	}
#endif

	/// <summary>Provides data for the <see cref="KeypadEventConnector.ButtonPressed"/> and <see cref="KeypadEventConnector.ButtonReleased"/> events.</summary>
	public class KeypadButtonEventArgs : EventArgs {
		/// <summary>Returns the index of the button that generated the event.</summary>
		public int ButtonIndex { get; }
		/// <summary>If set to true in a <see cref="KeypadEventConnector.ButtonPressed"/> event, the button will stay down.</summary>
		public bool StayDown { get; set; }
		/// <summary>If set to true in a <see cref="KeypadEventConnector.ButtonPressed"/> event, the button will not be automatically released when the animation finishes.</summary>
		public bool SuppressAutomaticRelease { get; set; }

		public KeypadButtonEventArgs(int buttonIndex) {
			this.ButtonIndex = buttonIndex;
		}
	}
}
