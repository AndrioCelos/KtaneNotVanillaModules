using System;

namespace NotVanillaModulesLib {
#if (!DEBUG)
	/// <summary>Passes events from a wire component to a Not Vanilla module. This class is not available in debug builds.</summary>
	internal class WireEventConnector : IWireParent {
		/// <summary>Called when a wire attached to this instance is cut.</summary>
		public event EventHandler<WireCutEventArgs> WireCut;

		/// <summary>Attaches this instance to the specified <see cref="SnippableWire"/>, allowing events to be received from the wire.</summary>
		public void Attach(SnippableWire wire) => wire.ParentComponent = this;
		/// <summary>Attaches this instance to the specified <see cref="VennSnippableWire"/>, allowing events to be received from the wire.</summary>
		public void Attach(VennSnippableWire wire) => wire.ParentComponent = this;
		/// <summary>Attaches this instance to the specified <see cref="WireSequenceWire"/>, allowing events to be received from the wire.</summary>
		public void Attach(WireSequenceWire wire) => wire.ParentComponent = this;

		void IWireParent.WireSnipped(int index) => this.WireCut?.Invoke(this, new WireCutEventArgs(index));
	}
#endif
}
