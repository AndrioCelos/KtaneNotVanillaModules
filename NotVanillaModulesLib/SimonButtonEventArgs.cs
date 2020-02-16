using System;

namespace NotVanillaModulesLib {
	public class SimonButtonEventArgs : EventArgs {
		public SimonButtons Colour { get; }

		public SimonButtonEventArgs(SimonButtons colour) {
			this.Colour = colour;
		}
	}
}