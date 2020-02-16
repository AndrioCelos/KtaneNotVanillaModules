using System;

namespace NotVanillaModulesLib {
#if (!DEBUG)
	/// <summary>Passes events from a <see cref="PressableButton"/> to a Not Vanilla module. This class is not available in debug builds.</summary>
	// FalseButtonConnector itself can't be used for this because the interface causes Unity to fail to load it.</remarks>
	internal class ButtonEventConnector : PressableButton.IParent {
		/// <summary>Called when a button attached to this instance is pressed.</summary>
		public event EventHandler Held;
		/// <summary>Called when a button attached to this instance is released.</summary>
		public event EventHandler Released;

		/// <summary>Attaches this instance to the specified <see cref="PressableButton"/>, allowing events to be received from the button.</summary>
		public void Attach(PressableButton button) => button.parentComponent = this;

		void PressableButton.IParent.OnHold() => this.Held?.Invoke(this, EventArgs.Empty);
		void PressableButton.IParent.OnRelease() => this.Released?.Invoke(this, EventArgs.Empty);
	}
#endif
}
