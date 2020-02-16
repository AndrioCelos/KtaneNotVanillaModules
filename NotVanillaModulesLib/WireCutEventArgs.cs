using System;

namespace NotVanillaModulesLib {
	/// <summary>Provides data for the <see cref="WireEventConnector.WireCut"/> event.</summary>
	public class WireCutEventArgs : EventArgs {
		/// <summary>Returns the index of the wire that was cut.</summary>
		public int WireIndex { get; }

		public WireCutEventArgs(int wireIndex) {
			this.WireIndex = wireIndex;
		}
	}
}