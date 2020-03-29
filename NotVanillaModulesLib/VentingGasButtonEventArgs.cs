using System;

namespace NotVanillaModulesLib {
	public class VentingGasButtonEventArgs : EventArgs {
		public VentingGasButton Button { get; }
		public VentingGasButtonEventArgs(VentingGasButton button) => this.Button = button;
	}
}